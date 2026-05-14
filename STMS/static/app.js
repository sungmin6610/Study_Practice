let inventoryData = [];
let categoryChartInstance = null;

// Initialization
document.addEventListener('DOMContentLoaded', () => {
    initNavigation();
    initFileUpload();
    initManualToggle();
    initModal();
    initOperations();
    
    // Load initial data
    fetchDashboard();
    fetchInventory();
    
    document.getElementById('refresh-btn').addEventListener('click', () => {
        fetchInventory();
        showToast('데이터를 새로고침했습니다.', 'success');
    });

    document.getElementById('search-input').addEventListener('input', (e) => {
        renderInventoryTable(e.target.value);
    });
});

// Navigation
function initNavigation() {
    const navBtns = document.querySelectorAll('.nav-btn');
    const sections = document.querySelectorAll('.view-section');

    navBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            // Update active button
            navBtns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            // Show target section
            const targetId = btn.getAttribute('data-target');
            sections.forEach(sec => sec.classList.remove('active'));
            document.getElementById(targetId).classList.add('active');
            
            if (targetId === 'dashboard') fetchDashboard();
            if (targetId === 'inventory') fetchInventory();
        });
    });
}

// Fetch APIs
async function fetchDashboard() {
    try {
        const res = await fetch('/api/dashboard');
        const data = await res.json();
        
        document.getElementById('dash-total-stock').textContent = data.total_stock.toLocaleString();
        document.getElementById('dash-total-items').textContent = data.total_items.toLocaleString();
        document.getElementById('dash-low-stock').textContent = data.low_stock_items.length;
        
        renderCategoryChart(data.categories);
        renderLowStockList(data.low_stock_items);
    } catch (err) {
        console.error('Failed to fetch dashboard:', err);
    }
}

async function fetchInventory() {
    try {
        const res = await fetch('/api/inventory');
        const data = await res.json();
        inventoryData = data.inventory;
        renderInventoryTable();
    } catch (err) {
        console.error('Failed to fetch inventory:', err);
    }
}

// Render UI Components
function renderCategoryChart(categories) {
    const ctx = document.getElementById('categoryChart').getContext('2d');
    
    if (categoryChartInstance) {
        categoryChartInstance.destroy();
    }
    
    const labels = Object.keys(categories).map(k => k.replace('납품 빈도 : ', ''));
    const data = Object.values(categories);
    
    categoryChartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: [
                    '#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899', '#14b8a6'
                ],
                borderWidth: 0,
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'right', labels: { color: '#e2e8f0' } }
            },
            cutout: '70%'
        }
    });
}

function renderLowStockList(items) {
    const list = document.getElementById('low-stock-list');
    list.innerHTML = '';
    
    if (items.length === 0) {
        list.innerHTML = '<p style="color: var(--text-muted);">부족한 재고가 없습니다.</p>';
        return;
    }
    
    items.forEach(item => {
        const partNo = item.new_part_no || item.old_part_no;
        const div = document.createElement('div');
        div.className = 'low-stock-item';
        div.innerHTML = `
            <div>
                <strong>${partNo}</strong>
                <div style="font-size: 12px; color: var(--text-muted);">${item.name || '이름 없음'}</div>
            </div>
            <div style="color: var(--danger); font-weight: bold;">
                ${item.stock} 개
            </div>
        `;
        list.appendChild(div);
    });
}

function renderInventoryTable(filterText = '') {
    const tbody = document.getElementById('inventory-tbody');
    tbody.innerHTML = '';
    
    const lowerFilter = filterText.toLowerCase();
    
    const filtered = inventoryData.filter(item => {
        return item.new_part_no.toLowerCase().includes(lowerFilter) ||
               item.old_part_no.toLowerCase().includes(lowerFilter) ||
               item.name.toLowerCase().includes(lowerFilter);
    });
    
    filtered.forEach(item => {
        const tr = document.createElement('tr');
        
        let stockColor = '';
        if (item.stock < 50) stockColor = 'color: var(--danger); font-weight: bold;';
        
        tr.innerHTML = `
            <td>${item.category.replace('납품 빈도 : ', '')}</td>
            <td>${item.new_part_no}</td>
            <td>${item.old_part_no}</td>
            <td title="${item.name}">${item.name.length > 30 ? item.name.substring(0, 30) + '...' : item.name}</td>
            <td style="${stockColor}">${item.stock.toLocaleString()}</td>
            <td>${item.in_progress.toLocaleString()}</td>
        `;
        tbody.appendChild(tr);
    });
}

// File Upload
let selectedFile = null;
function initFileUpload() {
    const dropZone = document.getElementById('drop-zone');
    const fileInput = document.getElementById('delivery-file');
    const fileNameDisplay = document.getElementById('file-name-display');
    const uploadBtn = document.getElementById('upload-btn');

    dropZone.addEventListener('click', () => fileInput.click());

    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('dragover');
    });

    dropZone.addEventListener('dragleave', () => dropZone.classList.remove('dragover'));

    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('dragover');
        if (e.dataTransfer.files.length) {
            handleFileSelect(e.dataTransfer.files[0]);
        }
    });

    fileInput.addEventListener('change', (e) => {
        if (e.target.files.length) {
            handleFileSelect(e.target.files[0]);
        }
    });

    function handleFileSelect(file) {
        selectedFile = file;
        fileNameDisplay.textContent = `선택된 파일: ${file.name}`;
        uploadBtn.disabled = false;
    }

    uploadBtn.addEventListener('click', async () => {
        if (!selectedFile) return;
        
        uploadBtn.disabled = true;
        uploadBtn.textContent = '처리 중...';
        
        const formData = new FormData();
        formData.append('file', selectedFile);
        
        try {
            const res = await fetch('/api/upload-delivery', {
                method: 'POST',
                body: formData
            });
            
            const result = await res.json();
            if (res.ok) {
                showToast(`성공적으로 업데이트되었습니다. (${result.updates.length}건 반영)`, 'success');
                // Reset
                selectedFile = null;
                fileNameDisplay.textContent = '';
                uploadBtn.textContent = '업로드 및 반영';
                fileInput.value = '';
                
                // Refresh data
                fetchDashboard();
                fetchInventory();
            } else {
                throw new Error(result.detail || '업로드 실패');
            }
        } catch (err) {
            showToast(err.message, 'error');
            uploadBtn.disabled = false;
            uploadBtn.textContent = '업로드 및 반영';
        }
    });
}

// Operations
function initOperations() {
    // Add Production
    document.getElementById('add-prod-btn').addEventListener('click', async () => {
        const partNo = document.getElementById('prod-part-no').value;
        const qty = document.getElementById('prod-qty').value;
        
        if (!partNo || !qty) {
            showToast('품번과 수량을 입력해주세요.', 'error');
            return;
        }
        
        try {
            const res = await fetch('/api/add-production', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ part_no: partNo, qty: parseInt(qty) })
            });
            
            const result = await res.json();
            if (res.ok) {
                showToast(`${partNo}의 재고가 ${qty}개 추가되었습니다. (현재 재고: ${result.new_stock})`, 'success');
                document.getElementById('prod-part-no').value = '';
                document.getElementById('prod-qty').value = '';
                fetchDashboard();
            } else {
                throw new Error(result.detail || '추가 실패');
            }
        } catch (err) {
            showToast(err.message, 'error');
        }
    });
}

// Manual Override
function initManualToggle() {
    const toggle = document.getElementById('manual-toggle');
    const controls = document.getElementById('manual-controls');
    const inputs = controls.querySelectorAll('input, button');
    
    toggle.addEventListener('change', (e) => {
        if (e.target.checked) {
            controls.classList.add('active');
            inputs.forEach(i => i.disabled = false);
        } else {
            controls.classList.remove('active');
            inputs.forEach(i => i.disabled = true);
        }
    });
}

function initModal() {
    const modal = document.getElementById('warning-modal');
    const manualBtn = document.getElementById('manual-btn');
    const cancelBtn = document.getElementById('modal-cancel');
    const confirmBtn = document.getElementById('modal-confirm');
    
    manualBtn.addEventListener('click', () => {
        const partNo = document.getElementById('manual-part-no').value;
        const qty = document.getElementById('manual-qty').value;
        
        if (!partNo || qty === '') {
            showToast('품번과 수량을 확인해주세요.', 'error');
            return;
        }
        
        document.getElementById('modal-part').textContent = partNo;
        document.getElementById('modal-qty').textContent = qty;
        modal.classList.add('show');
    });
    
    cancelBtn.addEventListener('click', () => {
        modal.classList.remove('show');
    });
    
    confirmBtn.addEventListener('click', async () => {
        modal.classList.remove('show');
        
        const partNo = document.getElementById('manual-part-no').value;
        const qty = document.getElementById('manual-qty').value;
        
        try {
            const res = await fetch('/api/manual-adjustment', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ part_no: partNo, new_stock: parseInt(qty) })
            });
            
            const result = await res.json();
            if (res.ok) {
                showToast(`강제 수정 완료. (이전: ${result.old_stock} → 현재: ${result.new_stock})`, 'success');
                document.getElementById('manual-part-no').value = '';
                document.getElementById('manual-qty').value = '';
                fetchDashboard();
            } else {
                throw new Error(result.detail || '수정 실패');
            }
        } catch (err) {
            showToast(err.message, 'error');
        }
    });
}

function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    toast.textContent = message;
    toast.className = `toast show ${type}`;
    
    setTimeout(() => {
        toast.className = toast.className.replace('show', '');
    }, 3000);
}
