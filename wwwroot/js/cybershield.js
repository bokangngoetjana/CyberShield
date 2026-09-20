// ---------- Shared helper to render a result card ----------
function renderResult(containerId, result) {
    const container = document.getElementById(containerId);
    container.classList.remove('d-none');

    const riskConfig = {
        Green: { icon: '✓', rowClass: 'risk-green' },
        Orange: { icon: '!', rowClass: 'risk-orange' },
        Red: { icon: '×', rowClass: 'risk-red' }
    };
    const cfg = riskConfig[result.riskLevel] || { icon: 'i', rowClass: '' };

    const signalsHtml = result.signals && result.signals.length
        ? result.signals.map(s => `<li><span class="signal-dot" aria-hidden="true">•</span><span>${escapeHtml(s)}</span></li>`).join('')
        : '<li>No specific signals detected.</li>';

    container.innerHTML = `
        <div class="card result-card ${cfg.rowClass}">
            <div class="card-body p-4">
                <div class="d-flex align-items-center justify-content-between mb-3 flex-wrap gap-2">
                    <span class="badge risk-badge"><span aria-hidden="true">${cfg.icon}</span> ${escapeHtml(result.riskLevel.toUpperCase())}</span>
                    <span class="trust-score"><span class="trust-label">Trust score</span>${result.trustScore}<span class="text-muted fs-6">/100</span></span>
                </div>
                <p class="mb-3 fs-6">${escapeHtml(result.summary)}</p>
                <ul class="list-unstyled signal-list mb-0">${signalsHtml}</ul>
            </div>
        </div>
    `;
}

function escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

function toggleSpinner(spinnerId, show) {
    document.getElementById(spinnerId).classList.toggle('d-none', !show);
}

// ---------- Message Scanner ----------
document.getElementById('scanMessageBtn').addEventListener('click', async () => {
    const text = document.getElementById('messageInput').value.trim();
    if (!text) { alert('Please enter a message to scan.'); return; }

    toggleSpinner('messageSpinner', true);
    document.getElementById('messageResult').classList.add('d-none');

    try {
        const response = await fetch('/api/MessageScanner/scan', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ text })
        });

        if (!response.ok) throw new Error(`Request failed: ${response.status}`);

        const result = await response.json();
        renderResult('messageResult', result);
    } catch (err) {
        alert('Something went wrong scanning the message: ' + err.message);
    } finally {
        toggleSpinner('messageSpinner', false);
    }
});

document.getElementById('sampleScamBtn').addEventListener('click', () => {
    document.getElementById('messageInput').value =
        "URGENT: Your bank account will be suspended in 24 hours. Click here to verify your identity immediately: bit.ly/xyz123";
});

// ---------- URL Checker ----------
document.getElementById('checkUrlBtn').addEventListener('click', async () => {
    const url = document.getElementById('urlInput').value.trim();
    if (!url) { alert('Please enter a URL to check.'); return; }

    toggleSpinner('urlSpinner', true);
    document.getElementById('urlResult').classList.add('d-none');

    try {
        const response = await fetch('/api/LinkCheck/check-url', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ url })
        });

        if (!response.ok) throw new Error(`Request failed: ${response.status}`);

        const result = await response.json();
        renderResult('urlResult', result);
    } catch (err) {
        alert('Something went wrong checking the URL: ' + err.message);
    } finally {
        toggleSpinner('urlSpinner', false);
    }
});

document.getElementById('sampleUrlBtn').addEventListener('click', () => {
    document.getElementById('urlInput').value = "https://testsafebrowsing.appspot.com/s/malware.html";
});

// ---------- Image Checker ----------
document.getElementById('checkImageBtn').addEventListener('click', async () => {
    const fileInput = document.getElementById('imageInput');
    if (!fileInput.files || fileInput.files.length === 0) {
        alert('Please choose an image file first.');
        return;
    }

    toggleSpinner('imageSpinner', true);
    document.getElementById('imageResult').classList.add('d-none');

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);

    try {
        const response = await fetch('/api/DocumentCheck/check-image', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) throw new Error(`Request failed: ${response.status}`);

        const result = await response.json();
        renderResult('imageResult', result);
    } catch (err) {
        alert('Something went wrong checking the image: ' + err.message);
    } finally {
        toggleSpinner('imageSpinner', false);
    }
});