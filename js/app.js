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
    initCategoryFilters();
    initScrollEffects();
});

async function loadInitialData() {
    try {
        // جلب المشاريع وتخزينها في الذاكرة الثابتة
        const allBusinesses = await E7_DB.getBusinesses();
        // فقط الشركاء النشطين (الذين تم قبولهم ودفعوا) يظهرون للزبائن
        cachedBusinesses = allBusinesses.filter(b => b.status === 'active' || !b.status);
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

    grid.textContent = ''; // clear grid

    if (!data || data.length === 0) {
        const emptyDiv = document.createElement('div');
        emptyDiv.style.cssText = 'grid-column: 1 / -1; text-align:center; padding: 5rem 0;';
        
        const ghostIcon = document.createElement('i');
        ghostIcon.className = 'fa-solid fa-ghost';
        ghostIcon.style.cssText = 'font-size: 3rem; opacity: 0.1; margin-bottom: 1rem; display: block;';
        emptyDiv.appendChild(ghostIcon);

        const emptyP = document.createElement('p');
        emptyP.textContent = 'لا توجد نتائج تطابق بحثك حالياً.';
        emptyDiv.appendChild(emptyP);

        grid.appendChild(emptyDiv);
        return;
    }

    data.forEach(biz => {
        const card = document.createElement('div');
        card.className = 'business-card fade-in';

        // Badge
        const badge = document.createElement('div');
        badge.className = 'card-badge';
        badge.textContent = 'متوفر الآن';
        card.appendChild(badge);

        // Image section
        const cardImage = document.createElement('div');
        cardImage.className = 'card-image';

        const img = document.createElement('img');
        img.src = biz.img || biz.imageUrl || 'img/logo.png';
        img.alt = biz.name;
        cardImage.appendChild(img);

        const fav = document.createElement('div');
        fav.className = 'card-fav';
        const heart = document.createElement('i');
        heart.className = 'fa-regular fa-heart';
        fav.appendChild(heart);
        cardImage.appendChild(fav);

        card.appendChild(cardImage);

        // Content section
        const cardContent = document.createElement('div');
        cardContent.className = 'card-content';

        // Meta info row
        const metaRow = document.createElement('div');
        metaRow.style.cssText = 'display:flex; justify-content:space-between; margin-bottom: 0.5rem; font-size: 0.85rem;';
        
        const categorySpan = document.createElement('span');
        categorySpan.style.cssText = 'color:var(--primary); font-weight:700;';
        categorySpan.textContent = biz.category;
        metaRow.appendChild(categorySpan);

        const ratingSpan = document.createElement('span');
        const starIcon = document.createElement('i');
        starIcon.className = 'fa-solid fa-star';
        starIcon.style.color = '#fbbf24';
        ratingSpan.appendChild(starIcon);
        ratingSpan.appendChild(document.createTextNode(' 4.9'));
        metaRow.appendChild(ratingSpan);

        cardContent.appendChild(metaRow);

        // Title
        const title = document.createElement('h3');
        title.textContent = biz.name;
        cardContent.appendChild(title);

        // Location
        const locPara = document.createElement('p');
        locPara.style.cssText = 'font-size:0.9rem; color:var(--text-muted);';
        const locIcon = document.createElement('i');
        locIcon.className = 'fa-solid fa-location-dot';
        locPara.appendChild(locIcon);
        locPara.appendChild(document.createTextNode(` ${biz.location}`));
        cardContent.appendChild(locPara);

        // Card footer (button & info)
        const cardMeta = document.createElement('div');
        cardMeta.className = 'card-meta';

        const priceDiv = document.createElement('div');
        priceDiv.className = 'card-price';
        priceDiv.style.cssText = 'font-size: 0.9rem; color: var(--text-muted);';
        const checkIcon = document.createElement('i');
        checkIcon.className = 'fa-solid fa-calendar-check';
        priceDiv.appendChild(checkIcon);
        
        const availabilitySpan = document.createElement('span');
        availabilitySpan.textContent = ' متاح للحجز';
        priceDiv.appendChild(availabilitySpan);
        cardMeta.appendChild(priceDiv);

        const bookBtn = document.createElement('button');
        bookBtn.className = 'btn btn-primary';
        bookBtn.textContent = 'احجز الآن';
        bookBtn.onclick = function() {
            window.location.href = `business-details.html?id=${biz.id}`;
        };
        cardMeta.appendChild(bookBtn);

        cardContent.appendChild(cardMeta);
        card.appendChild(cardContent);
        grid.appendChild(card);
    });
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

function initMobileMenu() {
    const toggle = document.getElementById('mobileToggle');
    const menu = document.getElementById('navMenu');
    
    if (toggle && menu) {
        toggle.addEventListener('click', (e) => {
            e.stopPropagation();
            menu.classList.toggle('active');
            toggle.querySelector('i').classList.toggle('fa-bars');
            toggle.querySelector('i').classList.toggle('fa-xmark');
        });

        // إغلاق القائمة عند الضغط في أي مكان خارجها
        document.addEventListener('click', (e) => {
            if (!menu.contains(e.target) && !toggle.contains(e.target)) {
                menu.classList.remove('active');
                toggle.querySelector('i').classList.add('fa-bars');
                toggle.querySelector('i').classList.remove('fa-xmark');
            }
        });
    }
}
function initCategoryFilters() {
    const categories = document.querySelectorAll('.category-item');
    categories.forEach(item => {
        item.addEventListener('click', () => {
            // إضافة كلاس Active للشكل الجمالي
            categories.forEach(c => c.classList.remove('active'));
            item.classList.add('active');

            const selectedCat = item.querySelector('span').innerText.toLowerCase();
            
            // فلترة البيانات بناءً على النص الموجود في الـ span
            const filtered = cachedBusinesses.filter(b => 
                b.category.toLowerCase().includes(selectedCat) || 
                selectedCat.includes(b.category.toLowerCase())
            );

            renderBusinesses(filtered);

            // النزول لنتائج البحث بسلاسة
            const featuredSection = document.getElementById('featured');
            if (featuredSection) featuredSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        });
    });
}

function initScrollEffects() {
    const sections = document.querySelectorAll('.section, .hero, .dashboard-card');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) entry.target.classList.add('visible');
        });
    }, { threshold: 0.1 });
    sections.forEach(s => observer.observe(s));
}
