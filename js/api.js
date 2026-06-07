// E7_DB Management System - PRODUCTION CLOUD VERSION
// This system connects to the .NET Backend hosted on Render & PostgreSQL on Supabase

const API_BASE_URL = (window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1" || window.location.protocol === "file:")
    ? "http://localhost:5123/api"
    : "https://e7jezli.onrender.com/api";

const E7_DB = {
    // جلب كافة المشاريع من السحاب
    getBusinesses: async () => {
        try {
            const response = await fetch(`${API_BASE_URL}/Business`);
            if (!response.ok) return [];
            return await response.json();
        } catch (e) {
            console.error("Cloud Database unreachable");
            return [];
        }
    },

    // جلب الحجوزات حسب معرف المستخدم (UserId)
    getBookingsByUser: async (userId) => {
        try {
            const url = `${API_BASE_URL}/Booking?userId=${userId}`;
            const response = await fetch(url);
            if (!response.ok) return [];
            return await response.json();
        } catch (e) {
            return [];
        }
    },

    // (اختياري) جلب الحجوزات حسب البريد الإلكتروني إذا كان الـ API يدعم ذلك
    getBookingsByEmail: async (email) => {
        try {
            let url = `${API_BASE_URL}/Booking`;
            if (email) {
                url += `?email=${encodeURIComponent(email)}`;
            }
            const response = await fetch(url);
            if (!response.ok) return [];
            return await response.json();
        } catch (e) {
            return [];
        }
    },
    
    // إضافة مشروع جديد للسحاب
    saveBusiness: async (biz) => {
        try {
            const response = await fetch(`${API_BASE_URL}/Business`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
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
                })
            });
            return await response.json();
        } catch (e) {
            console.error("Failed to save to Cloud");
            throw e;
        }
    },
    
    // تأكيد حجز جديد
    saveBooking: async (booking) => {
        try {
            const response = await fetch(`${API_BASE_URL}/Booking`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(booking)
            });
            if (!response.ok) throw new Error("فشل إرسال طلب الحجز");
            return await response.json();
        } catch (e) {
            console.error("Booking failed", e);
            throw e;
        }
    },

    // حذف مشروع من السحاب
    deleteBusiness: async (id) => {
        try {
            await fetch(`${API_BASE_URL}/Business/${id}`, { method: 'DELETE' });
        } catch (e) {
            console.error("Delete failed");
        }
    },

    // جلب تفاصيل مشروع معين
    getBusinessById: async (id) => {
        try {
            const list = await E7_DB.getBusinesses();
            return list.find(b => b.id == id) || null;
        } catch (e) {
            return null;
        }
    },

    // تحديث حالة الحجز
    updateBookingStatus: async (id, status) => {
        try {
            await fetch(`${API_BASE_URL}/Booking/${id}/status`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(status)
            });
        } catch (e) {
            console.error("Failed to update status");
        }
    },

    // حذف حجز من السحاب
    deleteBooking: async (id) => {
        try {
            await fetch(`${API_BASE_URL}/Booking/${id}`, { method: 'DELETE' });
        } catch (e) {
            console.error("Delete booking failed");
        }
    },

    // تحديث حالة الشريك
    updateBusinessStatus: async (id, status) => {
        try {
            await fetch(`${API_BASE_URL}/Business/${id}/status`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(status)
            });
        } catch (e) {
            console.error("Failed to update business status");
        }
    },

    // تسجيل حساب مستخدم جديد
    registerUser: async (fullName, email, password) => {
        const response = await fetch(`${API_BASE_URL}/Auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ fullName, email, password })
        });
        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || "فشل تسجيل الحساب");
        }
        return await response.json();
    },

    // تسجيل دخول المستخدم
    loginUser: async (email, password) => {
        const response = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || "البريد الإلكتروني أو كلمة المرور غير صحيحة");
        }
        return await response.json();
    },

    // جلب ملف تعريف المستخدم
    getUserProfile: async (email) => {
        const response = await fetch(`${API_BASE_URL}/Auth/profile?email=${encodeURIComponent(email)}`);
        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || "فشل جلب بيانات المستخدم");
        }
        return await response.json();
    },

    // تغيير كلمة المرور
    changePassword: async (email, oldPassword, newPassword) => {
        const response = await fetch(`${API_BASE_URL}/Auth/change-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, oldPassword, newPassword })
        });
        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || "فشل تغيير كلمة المرور");
        }
        return await response.json();
    },

    // إعادة تعيين كلمة المرور (من دون كلمة قديمة)
    resetPassword: async (email, newPassword) => {
        const response = await fetch(`${API_BASE_URL}/Auth/reset-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, newPassword })
        });
        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || "فشل إعادة تعيين كلمة المرور");
        }
        return await response.json();
    },
};
