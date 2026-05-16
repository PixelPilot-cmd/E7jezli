// E7_DB Management System - PURE CLOUD VERSION
// This system now connects exclusively to the .NET Backend & PostgreSQL Cloud Database

const API_BASE_URL = "https://localhost:7123/api";

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
            const response = await fetch(`${API_BASE_URL}/Booking`);
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
                    imageUrl: biz.img, // Mapping 'img' from frontend to 'imageUrl' in backend
                    facebookLink: biz.social?.fb,
                    instagramLink: biz.social?.ig,
                    whatsappLink: biz.social?.wa,
                    rating: 5.0,
                    status: "active"
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
            await fetch(`${API_BASE_URL}/Booking`, {
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
            const response = await fetch(`${API_BASE_URL}/Business/${id}`);
            if (response.ok) return await response.json();
            return null;
        } catch (e) {
            return null;
        }
    }
};
