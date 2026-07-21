const state = {
    nodes: [],
    activeProgramId: null,
    currentProgram: null,
    apiBaseUrl: null,
};

const elements = {};

function init() {
    cacheElements();
    bindEvents();
    updateRootPickCountVisibility();
    updateModalPickCountVisibility();
    renderNodes();
    clearResults();
    detectApiBaseUrl();
}

function cacheElements() {
    elements.programName = document.getElementById('programName');
    elements.rootGroupName = document.getElementById('rootGroupName');
    elements.rootGroupRule = document.getElementById('rootGroupRule');
    elements.rootPickCount = document.getElementById('rootPickCount');
    elements.rootPickCountWrap = document.getElementById('rootPickCountWrap');

    elements.nodesList = document.getElementById('nodesList');
    elements.nodesEmpty = document.getElementById('nodesEmpty');
    elements.btnAddNode = document.getElementById('btnAddNode');
    elements.btnCreate = document.getElementById('btnCreate');
    elements.btnValidate = document.getElementById('btnValidate');
    elements.btnGet = document.getElementById('btnGet');
    elements.btnClearAll = document.getElementById('btnClearAll');
    elements.btnLoadExample = document.getElementById('btnLoadExample');
    elements.btnClearResults = document.getElementById('btnClearResults');
    elements.btnCopyResponse = document.getElementById('btnCopyResponse');

    elements.responseOutput = document.getElementById('responseOutput');
    elements.responseSection = document.getElementById('responseSection');
    elements.treeSection = document.getElementById('treeSection');
    elements.treeContainer = document.getElementById('treeContainer');
    elements.resultsPlaceholder = document.getElementById('resultsPlaceholder');
    elements.currentProgram = document.getElementById('currentProgram');
    elements.currentProgramId = document.getElementById('currentProgramId');
    elements.validationBanner = document.getElementById('validationBanner');
    elements.apiStatus = document.getElementById('apiStatus');
    elements.loaderOverlay = document.getElementById('loaderOverlay');

    elements.modalOverlay = document.getElementById('modalOverlay');
    elements.modalTitle = document.getElementById('modalTitle');
    elements.modalNodeName = document.getElementById('modalNodeName');
    elements.modalGroupRule = document.getElementById('modalGroupRule');
    elements.modalPickCount = document.getElementById('modalPickCount');
    elements.modalPickCountWrap = document.getElementById('modalPickCountWrap');
    elements.modalPrereqSelect = document.getElementById('modalPrereqSelect');
    elements.modalConfirm = document.getElementById('modalConfirm');
    elements.modalCancel = document.getElementById('modalCancel');
    elements.modalClose = document.getElementById('modalClose');
    elements.toggleStep = document.getElementById('toggleStep');
    elements.toggleGroup = document.getElementById('toggleGroup');
}

function bindEvents() {
    elements.btnAddNode.addEventListener('click', openNodeModal);
    elements.modalCancel.addEventListener('click', closeNodeModal);
    elements.modalClose.addEventListener('click', closeNodeModal);
    elements.modalOverlay.addEventListener('click', (event) => {
        if (event.target === elements.modalOverlay) {
            closeNodeModal();
        }
    });
    elements.modalConfirm.addEventListener('click', handleAddNode);
    elements.toggleStep.addEventListener('click', () => setNodeType('step'));
    elements.toggleGroup.addEventListener('click', () => setNodeType('group'));
    elements.rootGroupRule.addEventListener('change', updateRootPickCountVisibility);
    elements.modalGroupRule.addEventListener('change', updateModalPickCountVisibility);

    elements.btnCreate.addEventListener('click', createProgram);
    elements.btnValidate.addEventListener('click', validateProgram);
    elements.btnGet.addEventListener('click', fetchProgram);
    elements.btnClearAll.addEventListener('click', clearAll);
    elements.btnLoadExample.addEventListener('click', loadExample);
    elements.btnClearResults.addEventListener('click', clearResults);
    elements.btnCopyResponse.addEventListener('click', copyResponse);
}

function setBusy(isBusy) {
    elements.loaderOverlay.style.display = isBusy ? 'flex' : 'none';
}

function setApiStatus(type, label) {
    const dot = elements.apiStatus.querySelector('.status-dot');
    const text = elements.apiStatus.querySelector('.status-label');
    dot.className = 'status-dot';

    if (type === 'online') {
        dot.classList.add('online');
    } else if (type === 'offline') {
        dot.classList.add('offline');
    }

    text.textContent = label;
}

async function detectApiBaseUrl() {
    const candidates = [
        window.location.origin,
        'http://localhost:5029',
        'https://localhost:7298',
        'http://localhost:5000',
        'http://localhost:5001',
    ];

    for (const candidate of candidates) {
        try {
            const response = await fetch(`${candidate}/swagger/v1/swagger.json`, { method: 'GET' });
            if (response.ok) {
                state.apiBaseUrl = candidate;
                setApiStatus('online', `Connected to ${candidate}`);
                return;
            }
        } catch (error) {
            // Ignore and try the next candidate.
        }
    }

    state.apiBaseUrl = 'http://localhost:5029';
    setApiStatus('offline', 'API offline');
}

async function requestJson(path, options = {}) {
    if (!state.apiBaseUrl) {
        await detectApiBaseUrl();
    }

    const url = `${state.apiBaseUrl}${path}`;
    const response = await fetch(url, {
        headers: {
            'Content-Type': 'application/json',
            ...(options.headers || {}),
        },
        ...options,
    });

    const text = await response.text();
    let data = null;
    if (text) {
        try {
            data = JSON.parse(text);
        } catch (error) {
            data = text;
        }
    }

    if (!response.ok) {
        const message = extractErrorMessage(data);
        throw new Error(message || `Request failed with status ${response.status}`);
    }

    return data;
}

function extractErrorMessage(data) {
    if (!data) {
        return 'Unexpected API response.';
    }

    if (typeof data === 'string') {
        return data;
    }

    if (Array.isArray(data)) {
        return data.join(', ');
    }

    if (data.errors && Array.isArray(data.errors)) {
        return data.errors.join(', ');
    }

    if (data.message) {
        return data.message;
    }

    if (data.title) {
        return data.title;
    }

    return JSON.stringify(data);
}

function collectProgramPayload() {
    const programName = elements.programName.value.trim() || 'Untitled Program';
    const rootName = elements.rootGroupName.value.trim() || programName;
    const rootRule = Number(elements.rootGroupRule.value);
    const rootPickCount = rootRule === 1 ? Number(elements.rootPickCount.value || 1) : null;

    const rootGroupId = createId();
    const rootGroup = {
        id: rootGroupId,
        name: rootName,
        isGroup: true,
        rule: rootRule,
        pickCount: rootPickCount,
        prerequisiteId: null,
        children: state.nodes.map((node) => toNodePayload(node, rootGroupId)),
    };

    return {
        name: programName,
        rootGroup,
    };
}

function toNodePayload(node, parentGroupId) {
    return {
        id: node.id,
        name: node.name,
        isGroup: node.isGroup,
        rule: node.isGroup ? node.rule : null,
        pickCount: node.isGroup ? node.pickCount : null,
        prerequisiteId: node.prerequisiteId || null,
        parentGroupId,
        children: [],
    };
}

function createId() {
    if (window.crypto && window.crypto.randomUUID) {
        return window.crypto.randomUUID();
    }
    return `node-${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

async function createProgram() {
    try {
        setBusy(true);
        const payload = collectProgramPayload();
        const created = await requestJson('/api/programs', {
            method: 'POST',
            body: JSON.stringify(payload),
        });

        state.activeProgramId = created.id;
        state.currentProgram = created;
        updateActionButtons(true);
        showResults(created, 'Program created successfully.');
        showToast('Program created successfully.', 'success');
    } catch (error) {
        showToast(error.message || 'Failed to create program.', 'error');
    } finally {
        setBusy(false);
    }
}

async function fetchProgram() {
    if (!state.activeProgramId) {
        showToast('Create or load a program first.', 'info');
        return;
    }

    try {
        setBusy(true);
        const program = await requestJson(`/api/programs/${state.activeProgramId}`);
        state.currentProgram = program;
        showResults(program, 'Program loaded successfully.');
        showToast('Program loaded successfully.', 'success');
    } catch (error) {
        showToast(error.message || 'Unable to fetch the program.', 'error');
    } finally {
        setBusy(false);
    }
}

async function validateProgram() {
    if (!state.activeProgramId) {
        showToast('Create or load a program first.', 'info');
        return;
    }

    try {
        setBusy(true);
        const result = await requestJson(`/api/programs/${state.activeProgramId}/validate`, { method: 'POST' });
        showValidationBanner(result);
        showToast('Validation completed.', 'success');
    } catch (error) {
        showToast(error.message || 'Validation failed.', 'error');
    } finally {
        setBusy(false);
    }
}

function showResults(program, message) {
    elements.resultsPlaceholder.style.display = 'none';
    elements.responseSection.style.display = 'block';
    elements.treeSection.style.display = 'block';
    elements.currentProgram.style.display = 'flex';
    elements.currentProgramId.textContent = state.activeProgramId || '—';
    elements.responseOutput.textContent = JSON.stringify(program, null, 2);
    elements.treeContainer.innerHTML = renderTree(program.rootGroup);
    elements.validationBanner.style.display = 'none';
    showToast(message, 'info');
}

function showValidationBanner(result) {
    const isValid = result?.isValid ?? false;
    const errors = result?.errors || [];
    const warnings = result?.warnings || [];
    const lines = [];

    if (isValid) {
        lines.push('<strong>Validation passed.</strong>');
        lines.push('The program structure satisfies the configured rules.');
    } else {
        lines.push('<strong>Validation failed.</strong>');
    }

    if (errors.length) {
        lines.push('<div><strong>Errors:</strong></div>');
        lines.push(`<ul>${errors.map((error) => `<li>${escapeHtml(error)}</li>`).join('')}</ul>`);
    }

    if (warnings.length) {
        lines.push('<div><strong>Warnings:</strong></div>');
        lines.push(`<ul>${warnings.map((warning) => `<li>${escapeHtml(warning)}</li>`).join('')}</ul>`);
    }

    elements.validationBanner.innerHTML = lines.join('');
    elements.validationBanner.style.display = 'block';
    elements.validationBanner.style.padding = '12px 14px';
    elements.validationBanner.style.borderRadius = '10px';
    elements.validationBanner.style.margin = '0 22px 14px';
    elements.validationBanner.style.background = isValid ? 'rgba(52,211,153,0.12)' : 'rgba(248,113,113,0.12)';
    elements.validationBanner.style.border = `1px solid ${isValid ? 'rgba(52,211,153,0.22)' : 'rgba(248,113,113,0.22)'}`;
    elements.validationBanner.style.color = isValid ? '#a7f3d0' : '#fecaca';
}

function renderTree(rootGroup) {
    if (!rootGroup) {
        return '<p class="nodes-empty">No tree data yet.</p>';
    }

    return `<div class="tree-node">
    <div class="tree-node-inner">
      <div class="tree-node-title">
        <span class="tree-node-icon">▣</span>
        <strong>${escapeHtml(rootGroup.name || 'Root Group')}</strong>
      </div>
      <div class="tree-node-meta">Group · ${rootGroup.rule === 1 ? 'Choice' : 'InOrder'}</div>
    </div>
    ${renderChildren(rootGroup.children || [])}
  </div>`;
}

function renderChildren(children) {
    if (!children || !children.length) {
        return '';
    }

    return `<div class="tree-node-children">${children.map((child) => renderNode(child)).join('')}</div>`;
}

function renderNode(node) {
    const icon = node.isGroup ? '▣' : '●';
    const typeLabel = node.isGroup ? 'Group' : 'Step';
    const meta = node.isGroup ? `${node.rule === 1 ? 'Choice' : 'InOrder'}` : 'Step';

    return `<div class="tree-node">
    <div class="tree-node-inner">
      <div class="tree-node-title">
        <span class="tree-node-icon">${icon}</span>
        <strong>${escapeHtml(node.name || 'Unnamed Node')}</strong>
      </div>
      <div class="tree-node-meta">${typeLabel} · ${meta}</div>
    </div>
    ${renderChildren(node.children || [])}
  </div>`;
}

function renderNodes() {
    if (!state.nodes.length) {
        elements.nodesList.innerHTML = `
      <div class="nodes-empty" id="nodesEmpty">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
        <p>No nodes yet. Click <strong>Add Node</strong> to begin.</p>
      </div>`;
        return;
    }

    elements.nodesList.innerHTML = state.nodes.map((node) => `
    <div class="node-item">
      <div class="node-icon ${node.isGroup ? 'group' : 'step'}">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          ${node.isGroup ? '<rect x="3" y="3" width="18" height="18" rx="2"></rect>' : '<path d="M5 12h14"></path>'}
        </svg>
      </div>
      <div class="node-info">
        <div class="node-name">${escapeHtml(node.name)}</div>
        <div class="node-meta">${node.isGroup ? 'Group' : 'Step'}${node.prerequisiteId ? ' · Prereq set' : ''}</div>
      </div>
      <button class="btn btn-ghost btn-xs remove-node" data-id="${node.id}">Remove</button>
    </div>`).join('');

    elements.nodesList.querySelectorAll('.remove-node').forEach((button) => {
        button.addEventListener('click', () => removeNode(button.dataset.id));
    });
}

function removeNode(nodeId) {
    state.nodes = state.nodes.filter((node) => node.id !== nodeId);
    renderNodes();
    showToast('Node removed.', 'info');
}

function openNodeModal() {
    resetModal();
    populatePrerequisiteOptions();
    elements.modalOverlay.classList.add('open');
    elements.modalNodeName.focus();
}

function closeNodeModal() {
    elements.modalOverlay.classList.remove('open');
    resetModal();
}

function resetModal() {
    elements.modalNodeName.value = '';
    elements.modalPrereqSelect.value = '';
    elements.modalGroupRule.value = '0';
    elements.modalPickCount.value = '1';
    setNodeType('step');
    updateModalPickCountVisibility();
}

function setNodeType(type) {
    elements.toggleStep.classList.toggle('active', type === 'step');
    elements.toggleGroup.classList.toggle('active', type === 'group');
    elements.toggleStep.setAttribute('aria-pressed', type === 'step');
    elements.toggleGroup.setAttribute('aria-pressed', type === 'group');
    elements.modalTitle.textContent = type === 'group' ? 'Add Group' : 'Add Node';
    elements.modalConfirm.textContent = type === 'group' ? 'Add Group' : 'Add Node';
    const groupOptions = document.getElementById('groupOptions');
    groupOptions.style.display = type === 'group' ? 'block' : 'none';
}

function populatePrerequisiteOptions() {
    const options = ['<option value="">— None —</option>'];
    state.nodes.forEach((node) => {
        options.push(`<option value="${node.id}">${escapeHtml(node.name)}</option>`);
    });
    elements.modalPrereqSelect.innerHTML = options.join('');
}

function handleAddNode() {
    const name = elements.modalNodeName.value.trim();
    if (!name) {
        showToast('Node name is required.', 'error');
        return;
    }

    const isGroup = elements.toggleGroup.classList.contains('active');
    const rule = Number(elements.modalGroupRule.value);
    const pickCount = isGroup && rule === 1 ? Number(elements.modalPickCount.value || 1) : null;

    state.nodes.push({
        id: createId(),
        name,
        isGroup,
        rule: isGroup ? rule : null,
        pickCount: isGroup ? pickCount : null,
        prerequisiteId: elements.modalPrereqSelect.value || null,
    });

    renderNodes();
    closeNodeModal();
    showToast(`${isGroup ? 'Group' : 'Node'} added.`, 'success');
}

function updateRootPickCountVisibility() {
    const show = Number(elements.rootGroupRule.value) === 1;
    elements.rootPickCountWrap.style.display = show ? 'block' : 'none';
}

function updateModalPickCountVisibility() {
    const show = Number(elements.modalGroupRule.value) === 1;
    elements.modalPickCountWrap.style.display = show ? 'block' : 'none';
}

function clearAll() {
    state.nodes = [];
    state.activeProgramId = null;
    state.currentProgram = null;
    elements.programName.value = '';
    elements.rootGroupName.value = '';
    elements.rootGroupRule.value = '0';
    elements.rootPickCount.value = '1';
    elements.currentProgram.style.display = 'none';
    elements.responseSection.style.display = 'none';
    elements.treeSection.style.display = 'none';
    elements.resultsPlaceholder.style.display = 'flex';
    elements.validationBanner.style.display = 'none';
    elements.responseOutput.textContent = '';
    elements.treeContainer.innerHTML = '';
    updateRootPickCountVisibility();
    renderNodes();
    updateActionButtons(false);
    showToast('Builder cleared.', 'info');
}

function updateActionButtons(enabled) {
    elements.btnValidate.disabled = !enabled;
    elements.btnGet.disabled = !enabled;
}

function clearResults() {
    elements.currentProgram.style.display = 'none';
    elements.responseSection.style.display = 'none';
    elements.treeSection.style.display = 'none';
    elements.resultsPlaceholder.style.display = 'flex';
    elements.validationBanner.style.display = 'none';
    elements.responseOutput.textContent = '';
    elements.treeContainer.innerHTML = '';
}

function copyResponse() {
    navigator.clipboard.writeText(elements.responseOutput.textContent).then(() => {
        showToast('Response copied to clipboard.', 'success');
    });
}

function loadExample() {
    elements.programName.value = 'Computer Science';
    elements.rootGroupName.value = 'Computer Science';
    elements.rootGroupRule.value = '0';
    elements.rootPickCount.value = '1';
    state.nodes = [
        { id: createId(), name: 'Programming Basics', isGroup: false, rule: null, pickCount: null, prerequisiteId: null },
        { id: createId(), name: 'Data Structures', isGroup: false, rule: null, pickCount: null, prerequisiteId: null },
        { id: createId(), name: 'Algorithms', isGroup: false, rule: null, pickCount: null, prerequisiteId: null },
    ];
    renderNodes();
    updateRootPickCountVisibility();
    showToast('Example program loaded.', 'info');
}

function showToast(message, type) {
    const container = document.getElementById('toastContainer');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
    <div class="toast-icon">${type === 'success' ? '✓' : type === 'error' ? '✕' : '•'}</div>
    <div>${escapeHtml(message)}</div>
  `;
    container.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('hide');
        setTimeout(() => toast.remove(), 250);
    }, 2500);
}

function escapeHtml(value) {
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

init();
