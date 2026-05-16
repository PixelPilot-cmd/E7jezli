// Global State - الذاكرة الثابتة للموقع
let cachedBusinesses = [];

document.addEventListener('DOMContentLoaded', async () => {
    // Hide Splash Screen
    const splash = document.getElementById('splash');
    if (splash) {
        setTimeout(() => {
            splash.style.opacity = '0';
            setTimeout(() => splash.style.display = 'none', 1000);
        }, 2000);
    }

    // جلب البيانات مرة واحدة فقط عند البداية لضمان الاستقرار والسرعة
    await loadInitialData();
    
    initSearch();
    initMobileMenu();
    initScrollEffects();
});

async function loadInitialData() {
    try {
        // جلب المشاريع وتخزينها في الذاكرة الثابتة
        cachedBusinesses = await E7_DB.getBusinesses();
        renderBusinesses(cachedBusinesses);
        
        // تحديث الإحصائيات
        const books = await E7_DB.getBookings();
        const totalB = document.getElementById('totalBookings') || document.getElementById('stBook');
        const totalP = document.getElementById('totalPartners') || document.getElementById('stBiz');
        if (totalB) totalB.innerText = books.length;
        if (totalP) totalP.innerText = cachedBusinesses.length;
    } catch (e) {
        console.error("خطأ في جلب البيانات الأولية:", e);
    }
}

function renderBusinesses(data) {
    const grid = document.getElementById('nearbyGrid');
    if (!grid) return;

    if (!data || data.length === 0) {
        grid.innerHTML = `
            <div style="grid-column: 1 / -1; text-align:center; padding: 5rem 0;">
                <i class="fa-solid fa-ghost" style="font-size: 3rem; opacity: 0.1; margin-bottom: 1rem; display: block;"></i>
                <p>لا توجد نتائج تطابق بحثك حالياً.</p>
            </div>
        `;
        return;
    }

    grid.innerHTML = data.map(biz => `
        <div class="business-card fade-in">
            <div class="card-badge">متوفر الآن</div>
            <div class="card-image">
                <img src="${biz.img || biz.imageUrl}" alt="${biz.name}">
                <div class="card-fav"><i class="fa-regular fa-heart"></i></div>
            </div>
            <div class="card-content">
                <div style="display:flex; justify-content:space-between; margin-bottom: 0.5rem; font-size: 0.85rem;">
                    <span style="color:var(--primary); font-weight:700;">${biz.category}</span>
                    <span><i class="fa-solid fa-star" style="color:#fbbf24;"></i> 4.9</span>
                </div>
                <h3>${biz.name}</h3>
                <p style="font-size:0.9rem; color:var(--text-muted);"><i class="fa-solid fa-location-dot"></i> ${biz.location}</p>
                <div class="card-meta">
                    <div class="card-price">80 <span>شيكل</span></div>
                    <button class="btn btn-primary" onclick="window.location.href='business-details.html?id=${biz.id}'">احجز الآن</button>
                </div>
            </div>
        </div>
    `).join('');
}

function initSearch() {
    const searchBtn = document.getElementById('mainSearchBtn');
    if (!searchBtn) return;

    searchBtn.addEventListener('click', () => {
        const service = document.getElementById('serviceSelect').value.toLowerCase();
        const city = document.getElementById('locationSelect').value;

        // البحث يتم فوراً من الذاكرة (ثابت وسريع جداً)
        const filtered = cachedBusinesses.filter(b => {
            const matchesService = service === "" || b.category.toLowerCase().includes(service);
            const matchesCity = city === "" || b.location === city;
            return matchesService && matchesCity;
        });

        renderBusinesses(filtered);
        
        const featuredSection = document.getElementById('featured');
        if (featuredSection) featuredSection.scrollIntoView({ behavior: 'smooth' });
    });
}

function initMobileMenu() {}
function initScrollEffects() {
    const sections = document.querySelectorAll('.section, .hero, .dashboard-card');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) entry.target.classList.add('visible');
        });
    }, { threshold: 0.1 });
    sections.forEach(s => observer.observe(s));
}
