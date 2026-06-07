// ============================================================
// E7jezli - نظام إدارة البيانات الهجين
// يعمل محلياً بالكامل + يزامن مع السيرفر السحابي عند الإمكان
// ============================================================

const API_BASE_URL = "https://e7jezli.onrender.com/api";

// ---- أدوات مساعدة ----
function _hash(str) {
    // SHA-256 simple simulation for localStorage (not cryptographic, just for matching)
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        const char = str.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash & hash;
    }
    return Math.abs(hash).toString(16).padStart(8, '0') + str.length.toString(16);
}

function _getUsers() {
    try { return JSON.parse(localStorage.getItem('_e7_users') || '[]'); } catch { return []; }
}
function _saveUsers(users) {
    localStorage.setItem('_e7_users', JSON.stringify(users));
}

function _getLocalBookings() {
    try { return JSON.parse(localStorage.getItem('_e7_bookings') || '[]'); } catch { return []; }
}
function _saveLocalBookings(bookings) {
    localStorage.setItem('_e7_bookings', JSON.stringify(bookings));
}

function _getLocalBusinesses() {
    try { return JSON.parse(localStorage.getItem('_e7_businesses') || '[]'); } catch { return []; }
}
function _saveLocalBusinesses(biz) {
    localStorage.setItem('_e7_businesses', JSON.stringify(biz));
}

// ---- محاولة الاتصال بالسيرفر (صامتة) ----
async function _tryFetch(url, options = {}, timeout = 10000) {
    try {
        const res = await fetch(url, { ...options, signal: AbortSignal.timeout(timeout) });
        return res;
    } catch (e) {
        console.warn(`Fetch failed for ${url}:`, e);
        return null;
    }
}

// ============================================================
// كائن E7_DB الرئيسي
// ============================================================
const E7_DB = {

    // ─────────────────────────────────────────────
    // AUTH - تسجيل / دخول / بروفايل / كلمة مرور
    // ─────────────────────────────────────────────

    registerUser: async (fullName, email, password) => {
        const emailNorm = email.trim().toLowerCase();
        const users = _getUsers();

        // منع التكرار محلياً
        if (users.find(u => u.email === emailNorm)) {
            throw new Error("البريد الإلكتروني مسجل بالفعل.");
        }

        const newUser = {
            id: Date.now(),
            fullName: fullName.trim(),
            email: emailNorm,
            passwordHash: _hash(password),
            points: 150,
            isPartner: false,
            dateCreated: new Date().toISOString()
        };

        // حفظ محلي فوري
        users.push(newUser);
        _saveUsers(users);

        // محاولة مزامنة مع السيرفر (صامتة)
        _tryFetch(`${API_BASE_URL}/Auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ fullName: newUser.fullName, email: newUser.email, password })
        }).then(async res => {
            if (res && res.ok) {
                const serverUser = await res.json().catch(() => null);
                if (serverUser && serverUser.id) {
                    // تحديث الـ ID بالـ ID السيرفر
                    const updatedUsers = _getUsers();
                    const idx = updatedUsers.findIndex(u => u.email === emailNorm);
                    if (idx !== -1) { updatedUsers[idx].serverId = serverUser.id; _saveUsers(updatedUsers); }
                }
            }
        }).catch(() => {});

        return newUser;
    },

    loginUser: async (email, password) => {
        const emailNorm = email.trim().toLowerCase();

        // ── 1. حاول السيرفر أولاً ──
        const res = await _tryFetch(`${API_BASE_URL}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: emailNorm, password })
        });

        if (res && res.ok) {
            const serverUser = await res.json();
            // حفظ/تحديث في localStorage
            const users = _getUsers();
            const idx = users.findIndex(u => u.email === emailNorm);
            const localUser = {
                id: serverUser.id,
                fullName: serverUser.fullName,
                email: emailNorm,
                passwordHash: _hash(password),
                points: serverUser.points || 0,
                isPartner: serverUser.isPartner || serverUser.IsPartner || false,
                dateCreated: serverUser.dateCreated || new Date().toISOString()
            };
            if (idx !== -1) { users[idx] = { ...users[idx], ...localUser }; }
            else { users.push(localUser); }
            _saveUsers(users);
            return localUser;
        }

        // ── 2. السيرفر غير متاح → استخدم localStorage ──
        const users = _getUsers();
        const user = users.find(u => u.email === emailNorm);
        if (!user) throw new Error("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        if (user.passwordHash !== _hash(password)) throw new Error("البريد الإلكتروني أو كلمة المرور غير صحيحة.");

        return user;
    },

    getUserProfile: async (email) => {
        const emailNorm = email.trim().toLowerCase();

        // حاول السيرفر
        const res = await _tryFetch(`${API_BASE_URL}/Auth/profile?email=${encodeURIComponent(emailNorm)}`);
        if (res && res.ok) {
            const serverUser = await res.json();
            // حدّث localStorage
            const users = _getUsers();
            const idx = users.findIndex(u => u.email === emailNorm);
            if (idx !== -1) {
                users[idx].points = serverUser.points || users[idx].points;
                users[idx].fullName = serverUser.fullName || users[idx].fullName;
                _saveUsers(users);
                return { ...users[idx], ...serverUser };
            }
            return serverUser;
        }

        // fallback محلي
        const users = _getUsers();
        const user = users.find(u => u.email === emailNorm);
        if (!user) throw new Error("المستخدم غير موجود.");
        return user;
    },

    changePassword: async (email, oldPassword, newPassword) => {
        const emailNorm = email.trim().toLowerCase();

        // حاول السيرفر
        const res = await _tryFetch(`${API_BASE_URL}/Auth/change-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: emailNorm, oldPassword, newPassword })
        });
        if (res && res.ok) {
            // حدّث كلمة المرور في localStorage أيضاً
            const users = _getUsers();
            const idx = users.findIndex(u => u.email === emailNorm);
            if (idx !== -1) { users[idx].passwordHash = _hash(newPassword); _saveUsers(users); }
            return await res.json();
        }

        // fallback محلي
        const users = _getUsers();
        const idx = users.findIndex(u => u.email === emailNorm);
        if (idx === -1) throw new Error("المستخدم غير موجود.");
        if (users[idx].passwordHash !== _hash(oldPassword)) throw new Error("كلمة المرور القديمة غير صحيحة.");
        users[idx].passwordHash = _hash(newPassword);
        _saveUsers(users);
        return { message: "تم تغيير كلمة المرور بنجاح." };
    },

    resetPassword: async (email, newPassword) => {
        const emailNorm = email.trim().toLowerCase();

        // حاول السيرفر
        const res = await _tryFetch(`${API_BASE_URL}/Auth/reset-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: emailNorm, newPassword })
        });
        if (res && res.ok) {
            const users = _getUsers();
            const idx = users.findIndex(u => u.email === emailNorm);
            if (idx !== -1) { users[idx].passwordHash = _hash(newPassword); _saveUsers(users); }
            return await res.json();
        }

        // fallback محلي
        const users = _getUsers();
        const idx = users.findIndex(u => u.email === emailNorm);
        if (idx === -1) throw new Error("البريد الإلكتروني غير مسجل في النظام.");
        users[idx].passwordHash = _hash(newPassword);
        _saveUsers(users);
        return { message: "تمت إعادة تعيين كلمة المرور بنجاح." };
    },

    redeemPoints: async (email) => {
        const emailNorm = email.trim().toLowerCase();

        // حاول السيرفر
        const res = await _tryFetch(`${API_BASE_URL}/Auth/redeem-points`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: emailNorm })
        });
        if (res && res.ok) {
            const result = await res.json();
            const users = _getUsers();
            const idx = users.findIndex(u => u.email === emailNorm);
            if (idx !== -1) { users[idx].points = result.points; _saveUsers(users); }
            return result;
        }

        // fallback محلي
        const users = _getUsers();
        const idx = users.findIndex(u => u.email === emailNorm);
        if (idx === -1) throw new Error("المستخدم غير موجود.");
        if ((users[idx].points || 0) < 1000) throw new Error("رصيد نقاطك غير كافٍ. تحتاج إلى 1000 نقطة على الأقل.");
        users[idx].points -= 1000;
        _saveUsers(users);
        return { points: users[idx].points, message: "تم استبدال النقاط بنجاح!" };
    },

    // تحديث نقاط المستخدم محلياً (يُستدعى بعد إتمام الحجز)
    _addPoints: (email, amount) => {
        const emailNorm = email.trim().toLowerCase();
        const users = _getUsers();
        const idx = users.findIndex(u => u.email === emailNorm);
        if (idx !== -1) {
            users[idx].points = (users[idx].points || 0) + amount;
            _saveUsers(users);
            // حدّث localStorage الجلسة
            const profile = JSON.parse(localStorage.getItem('user_profile') || 'null');
            if (profile && profile.email === emailNorm) {
                profile.points = users[idx].points;
                localStorage.setItem('user_profile', JSON.stringify(profile));
            }
            return users[idx].points;
        }
        return 0;
    },

    // ─────────────────────────────────────────────
    // BUSINESSES - المنشآت
    // ─────────────────────────────────────────────

    getBusinesses: async () => {
        const res = await _tryFetch(`${API_BASE_URL}/Business`);
        if (res && res.ok) {
            const data = await res.json();
            _saveLocalBusinesses(data); // cache
            return data;
        }
        // fallback
        return _getLocalBusinesses();
    },

    getBusinessById: async (id) => {
        try {
            const list = await E7_DB.getBusinesses();
            return list.find(b => b.id == id) || null;
        } catch { return null; }
    },

    saveBusiness: async (biz) => {
        const payload = {
            name: biz.name,
            location: biz.location,
            category: biz.category,
            imageUrl: biz.img,
            facebookLink: biz.social?.fb,
            instagramLink: biz.social?.ig,
            whatsappLink: biz.social?.wa,
            description: biz.description,
            secondaryImages: biz.secondaryImages,
            extraServices: biz.extraServices,
            rating: 5.0,
            status: "pending"
        };

        const res = await _tryFetch(`${API_BASE_URL}/Business`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res && res.ok) {
            const saved = await res.json();
            // حدّث الـ cache
            const local = _getLocalBusinesses();
            local.push(saved);
            _saveLocalBusinesses(local);
            return saved;
        }

        // fallback: حفظ محلي
        const localBiz = { ...payload, id: Date.now(), img: biz.img };
        const local = _getLocalBusinesses();
        local.push(localBiz);
        _saveLocalBusinesses(local);
        return localBiz;
    },

    deleteBusiness: async (id) => {
        _tryFetch(`${API_BASE_URL}/Business/${id}`, { method: 'DELETE' });
        // حذف محلي فوري
        const local = _getLocalBusinesses().filter(b => b.id != id);
        _saveLocalBusinesses(local);
    },

    updateBusinessStatus: async (id, status) => {
        _tryFetch(`${API_BASE_URL}/Business/${id}/status`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(status)
        });
        // تحديث محلي فوري
        const local = _getLocalBusinesses();
        const idx = local.findIndex(b => b.id == id);
        if (idx !== -1) { local[idx].status = status; _saveLocalBusinesses(local); }
    },

    // ─────────────────────────────────────────────
    // BOOKINGS - الحجوزات
    // ─────────────────────────────────────────────

    saveBooking: async (booking) => {
        // حفظ محلي فوري
        const local = _getLocalBookings();
        const localBooking = {
            ...booking,
            id: Date.now(),
            status: 'pending',
            dateCreated: new Date().toISOString(),
            _localOnly: true
        };
        local.push(localBooking);
        _saveLocalBookings(local);

        // محاولة مزامنة مع السيرفر (صامتة)
        _tryFetch(`${API_BASE_URL}/Booking`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(booking)
        }).then(async res => {
            if (res && res.ok) {
                const serverBooking = await res.json().catch(() => null);
                if (serverBooking) {
                    // استبدال النسخة المحلية بالسيرفر
                    const bookings = _getLocalBookings();
                    const idx = bookings.findIndex(b => b.id === localBooking.id);
                    if (idx !== -1) {
                        bookings[idx] = { ...serverBooking, _synced: true };
                        _saveLocalBookings(bookings);
                    }
                }
            }
        }).catch(() => {});

        return localBooking;
    },

    getBookings: async (email) => {
        // حاول السيرفر
        let url = `${API_BASE_URL}/Bookings`;
        if (email) url += `?email=${encodeURIComponent(email)}`;
        const res = await _tryFetch(url);

        if (res && res.ok) {
            let serverBookings = await res.json();
            
            // توحيد البيانات (Normalization) لضمان عمل camelCase
            serverBookings = serverBookings.map(b => ({
                id: b.id || b.Id,
                businessName: b.businessName || b.BusinessName,
                businessImage: b.businessImage || b.BusinessImage,
                service: b.service || b.Service,
                price: b.price || b.Price,
                time: b.time || b.Time,
                date: b.date || b.Date,
                userEmail: b.userEmail || b.UserEmail,
                status: b.status || b.Status,
                dateCreated: b.dateCreated || b.DateCreated
            }));

            // دمج مع البيانات المحلية غير المزامنة
            const local = _getLocalBookings().filter(lb => lb._localOnly && (!email || lb.userEmail === email));
            const merged = [...serverBookings];
            local.forEach(lb => {
                if (!merged.find(sb => sb.id === lb.id)) merged.push(lb);
            });
            // احفظ الدمج
            _saveLocalBookings(merged);
            return email ? merged.filter(b => b.userEmail?.toLowerCase() === email?.toLowerCase()) : merged;
        }

        // fallback محلي
        const local = _getLocalBookings();
        if (!email) return local;
        return local.filter(b => b.userEmail?.toLowerCase() === email.toLowerCase());
    },

    getBookingsByUser: async (userId) => {
        const res = await _tryFetch(`${API_BASE_URL}/Booking?userId=${userId}`);
        if (res && res.ok) return await res.json();
        return _getLocalBookings().filter(b => b.userId == userId);
    },

    getBookingsByEmail: async (email) => {
        return E7_DB.getBookings(email);
    },

    updateBookingStatus: async (id, status) => {
        // 1. تحديث محلي فوري لضمان تجربة مستخدم سريعة
        const local = _getLocalBookings();
        const idx = local.findIndex(b => (b.id || b.Id) == id);
        
        let userEmail = null;
        if (idx !== -1) {
            const booking = local[idx];
            userEmail = booking.userEmail || booking.UserEmail;
            booking.status = status;
            booking.Status = status;
            _saveLocalBookings(local);
            
            // إضافة نقاط الولاء محلياً فوراً إذا تم الإتمام
            if (status === 'completed' && userEmail) {
                E7_DB._addPoints(userEmail, 100);
            }
        }

        // 2. محاولة تحديث السيرفر (مع وقت انتظار طويل للاستيقاظ)
        try {
            const options = {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(status)
            };

            // ننتظر حتى 45 ثانية لاستيقاظ السيرفر
            let res = await _tryFetch(`${API_BASE_URL}/Booking/${id}/status`, options, 45000);
            if (!res || !res.ok) {
                res = await _tryFetch(`${API_BASE_URL}/Bookings/${id}/status`, options, 45000);
            }

            // إذا فشل التحديث تماماً على السيرفر، وكان الحجز ليس محلياً فقط
            if ((!res || !res.ok) && id < 1000000000000) { 
                throw new Error(res ? `Server error: ${res.status}` : "Server timeout (Sleeping)");
            }
            
            return true;
        } catch (e) {
            console.error("Sync error:", e);
            // لا نلقي الخطأ إذا كان الحجز محلياً فقط (لأنه سينجح محلياً على أي حال)
            if (id < 1000000000000) throw e; 
            return true;
        }
    },

    deleteBooking: async (id) => {
        // حذف محلي فوري
        const local = _getLocalBookings().filter(b => b.id != id);
        _saveLocalBookings(local);

        _tryFetch(`${API_BASE_URL}/Booking/${id}`, { method: 'DELETE' }).catch(() => {});
        _tryFetch(`${API_BASE_URL}/Bookings/${id}`, { method: 'DELETE' }).catch(() => {});
    },

    // ─────────────────────────────────────────────
    // ADMIN - لوحة الإدارة
    // ─────────────────────────────────────────────

    getAllBookingsAdmin: async () => {
        const res = await _tryFetch(`${API_BASE_URL}/Bookings`);
        if (res && res.ok) {
            const data = await res.json();
            _saveLocalBookings(data);
            return data;
        }
        return _getLocalBookings();
    }
};
