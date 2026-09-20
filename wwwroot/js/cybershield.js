// ---------- Shared helper to render a result card ----------
function renderResult(containerId, result) {
    const container = document.getElementById(containerId);
    container.classList.remove('d-none');

    const colorMap = { Green: 'success', Orange: 'warning', Red: 'danger' };
    const badgeColor = colorMap[result.riskLevel] || 'secondary';

    const signalsHtml = result.signals && result.signals.length
        ? result.signals.map(s => `<li>${escapeHtml(s)}</li>`).join('')
        : '<li>No specific signals detected.</li>';

    container.innerHTML = `
        <div class="card border-${badgeColor}">
            <div class="card-body">
                <div class="d-flex align-items-center mb-2">
                    <span class="badge bg-${badgeColor} fs-6 me-2">${result.riskLevel}</span>
                    <span class="fw-bold">Trust Score: ${result.trustScore}/100</span>
                </div>
                <p class="mb-2">${escapeHtml(result.summary)}</p>
                <ul class="mb-0">${signalsHtml}</ul>
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