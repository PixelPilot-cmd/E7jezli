// E7_DB Management System - PRODUCTION CLOUD VERSION
// This system connects to the .NET Backend hosted on Render & PostgreSQL on Supabase

const API_BASE_URL = "https://e7jezli.onrender.com/api";

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

    // جلب الحجوزات
    getBookings: async () => {
        try {
            const response = await fetch(`${API_BASE_URL}/Bookings`);
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
            await fetch(`${API_BASE_URL}/Bookings`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(booking)
            });
        } catch (e) {
            console.error("Booking failed");
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
            await fetch(`${API_BASE_URL}/Bookings/${id}/status`, {
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
            await fetch(`${API_BASE_URL}/Bookings/${id}`, { method: 'DELETE' });
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
    }
};
