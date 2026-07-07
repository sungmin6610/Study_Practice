/* ==========================================================================
   AMAT Centris Sym3 Etch Chamber Simulator - Application Logic
   ========================================================================== */

document.addEventListener('DOMContentLoaded', () => {

  // --- Constants ---
  const EQ_ID = "SYM3-ETCH-01";
  const RECIPE_NAME = "GAA_MAIN_ETCH_R3";
  const TARGET_DEPTH = 110.0; // nm (Endpoint Target, increased to allow full 100s run)
  const THIN_FILM_LIMIT = 85.0; // nm (Where OES drops)
  
  // Timing configuration for 10000 data points
  const DT = 0.01; // 0.01s process time per data point (100 Hz)
  const SUB_STEPS_PER_TICK = 100; // Generate 100 data points per UI tick
  const UI_TICK_MS = 100; // UI updates every 100ms in real-time (10x speedup)
  const MAX_DATA_POINTS = 10000;

  // --- Recipe Step Configurations ---
  // Total duration: 5 + 10 + 70 + 15 = 100 seconds
  let RECIPE_STEPS = [
    {
      stepNum: 1,
      name: "Strike",
      duration: 5,
      rfSource: 300,
      rfBias: 50,
      pressure: 30.0,
      cl2: 0,
      hbr: 0,
      ar: 50,
      targetEtchRate: 0.0,
      targetUniformity: 98.0,
      targetSelectivity: 0.0,
      plasmaColor: "url(#plasma-strike)"
    },
    {
      stepNum: 2,
      name: "Break-thru",
      duration: 10,
      rfSource: 600,
      rfBias: 100,
      pressure: 25.0,
      cl2: 80,
      hbr: 40,
      ar: 0,
      targetEtchRate: 30.0,
      targetUniformity: 96.0,
      targetSelectivity: 15.0,
      plasmaColor: "url(#plasma-etch)"
    },
    {
      stepNum: 3,
      name: "Main Etch",
      duration: 70, // Adjusted to 70s to reach 100s total
      rfSource: 900,
      rfBias: 150,
      pressure: 20.0,
      cl2: 120,
      hbr: 90,
      ar: 0,
      targetEtchRate: 60.0,
      targetUniformity: 97.0,
      targetSelectivity: 22.0,
      plasmaColor: "url(#plasma-etch)"
    },
    {
      stepNum: 4,
      name: "Over-Etch",
      duration: 15,
      rfSource: 850,
      rfBias: 140,
      pressure: 22.0,
      cl2: 100,
      hbr: 80,
      ar: 0,
      targetEtchRate: 15.0,
      targetUniformity: 96.0,
      targetSelectivity: 25.0,
      plasmaColor: "url(#plasma-etch)"
    }
  ];

  // --- State Variables ---
  let isRunning = false;
  let isPaused = false;
  let currentStepIndex = -1; // 0-based index
  let stepTimeElapsed = 0;
  let totalTimeElapsed = 0;
  let simulationStartTime = 0; // base real time
  
  // Recipe Editor State
  let selectedRecipeIndex = -1;

  // Realtime Telemetry Values
  let telemetry = {
    rfSource: 0,
    rfBias: 0,
    pressure: 0,
    cl2: 0,
    hbr: 0,
    ar: 0,
    temp: 65.0,
    etchRate: 0.0,
    etchDepth: 0.0,
    oesIntensity: 0.0,
    uniformity: 100.0,
    selectivity: 0.0,
    rfReflection: 0.0,
    status: "NORMAL",   // NORMAL, WARNING, FAULT, COMPLETE
    result: "OK"        // OK, NG
  };

  // Manual Override State
  let overrideMode = false;
  let manualInputs = {
    rfSource: 900,
    rfBias: 150,
    pressure: 20.0,
    cl2: 120,
    hbr: 90,
    temp: 65.0
  };

  // Simulation Loop Timer
  let simTimer = null;
  
  // Log Database (for CSV export)
  let logHistory = [];
  
  // Alarm History Data
  let alarmHistoryData = [];

  // Endpoint Detection (EPD) State
  let epdThreshold = 0.250;
  let epdState = "WAITING"; // WAITING, TRACKING, DETECTED
  let epdDetectedTime = -1;

  // Anomaly Injection Flags
  let activeAnomaly = null; // 'gas_low', 'rf_fault', or null
  let anomalySteps = 0;

  // --- DOM Elements ---
  const elEqId = document.getElementById('eq-id');
  const elRecipeName = document.getElementById('recipe-name');
  const elLotWafer = document.getElementById('lot-wafer');
  const elCommState = document.getElementById('comm-state');
  const elProcessState = document.getElementById('process-state');
  const elHealthState = document.getElementById('health-state');
  const elStepNumber = document.getElementById('step-number');
  
  // Chamber SVG elements
  const svgPlasmaGlow = document.getElementById('plasma-glow');
  const svgBiasGlow = document.getElementById('bias-glow');
  const svgCoilLeft = document.getElementById('coil-left');
  const svgCoilRight = document.getElementById('coil-right');
  const svgTurboFan = document.getElementById('turbo-fan');
  const svgValveFlap = document.getElementById('valve-flap');
  const svgWafer = document.getElementById('wafer');
  
  // SVG flows
  const flowCl2 = document.getElementById('flow-cl2');
  const flowHbr = document.getElementById('flow-hbr');
  const flowAr = document.getElementById('flow-ar');
  const flowO2 = document.getElementById('flow-o2');
  const flowMain = document.getElementById('flow-main');

  // Telemetry Overlays
  const elTelRefl = document.getElementById('tel-refl-power');
  const elTelPress = document.getElementById('tel-press');
  const elTelTemp = document.getElementById('tel-temp');

  // Controls & Inputs
  const toggleOverride = document.getElementById('override-toggle');
  const elModeText = document.getElementById('mode-text');
  const sectionManualInputs = document.getElementById('manual-inputs-section');

  const inRfSource = document.getElementById('input-rf-source');
  const valRfSource = document.getElementById('val-rf-source');
  const inRfBias = document.getElementById('input-rf-bias');
  const valRfBias = document.getElementById('val-rf-bias');
  const inCl2Flow = document.getElementById('input-cl2-flow-real');
  const valCl2Flow = document.getElementById('val-cl2-flow');
  const inHbrFlow = document.getElementById('input-hbr-flow-real');
  const valHbrFlow = document.getElementById('val-hbr-flow');
  const inPressure = document.getElementById('input-pressure');
  const inTemp = document.getElementById('input-temp');

  // Outputs / Cards
  const elUniformityGauge = document.getElementById('uniformity-gauge');
  const elValUniformity = document.getElementById('val-uniformity');
  const elUniformityStatus = document.getElementById('uniformity-status');
  const elValEtchRate = document.getElementById('val-etch-rate');
  const elValEtchDepth = document.getElementById('val-etch-depth');
  const elValTargetDepth = document.getElementById('val-target-depth');
  const elValSelectivity = document.getElementById('val-selectivity');
  const elFinalResultBadge = document.getElementById('final-result-badge');

  // Console / Buttons
  const consoleLog = document.getElementById('console-log');
  const btnClearConsole = document.getElementById('clear-console-btn');
  const selScenario = document.getElementById('scenario-select');
  
  const btnStart = document.getElementById('btn-start');
  const btnPause = document.getElementById('btn-pause');
  const btnStop = document.getElementById('btn-stop');
  const btnAbort = document.getElementById('btn-abort');
  const btnInjectFault = document.getElementById('btn-inject-fault');
  const btnSaveRecipe = document.getElementById('btn-save-recipe');
  const btnExportLog = document.getElementById('btn-export-log');
  const btnReset = document.getElementById('btn-reset');

  // Alarm Panel Elements
  const elAlarmCountBadge = document.getElementById('alarm-count-badge');
  const elFilterInfo = document.getElementById('filter-info');
  const elFilterWarning = document.getElementById('filter-warning');
  const elFilterError = document.getElementById('filter-error');
  const elFilterCritical = document.getElementById('filter-critical');
  const btnClearAlarms = document.getElementById('btn-clear-alarms');
  const elAlarmTableBody = document.getElementById('alarm-table-body');

  // EPD Elements
  const elEpdStatus = document.getElementById('epd-status');
  const elValEpdOes = document.getElementById('val-epd-oes');
  const elValEpdThreshold = document.getElementById('val-epd-threshold');
  const elValEpdTime = document.getElementById('val-epd-time');
  const elEpdProgressBar = document.getElementById('epd-progress-bar');
  const elEpdProgressText = document.getElementById('epd-progress-text');
  const elEpdPopupOverlay = document.getElementById('epd-popup-overlay');

  // Recipe Editor Toolbar Elements
  const elRecipeTableBody = document.getElementById('recipe-table-body');
  const btnAddStep = document.getElementById('btn-add-step');
  const btnDelStep = document.getElementById('btn-del-step');
  const btnDupStep = document.getElementById('btn-dup-step');
  const btnUpStep = document.getElementById('btn-up-step');
  const btnDownStep = document.getElementById('btn-down-step');
  const btnSaveLocal = document.getElementById('btn-save-local');
  const btnLoadLocal = document.getElementById('btn-load-local');
  const btnExportJson = document.getElementById('btn-export-json');
  const btnImportJson = document.getElementById('btn-import-json');
  const inputJsonUpload = document.getElementById('json-upload');

  // Set default target depth UI label
  elValTargetDepth.textContent = TARGET_DEPTH.toFixed(1);

  // --- Chart.js Configuration & Setup ---
  const ctx = document.getElementById('trendChart').getContext('2d');
  
  Chart.defaults.color = '#94a3b8';
  Chart.defaults.font.family = 'Inter';
  Chart.defaults.font.size = 9;

  // Custom Plugin to draw vertical line at EPD detection point
  const verticalLinePlugin = {
    id: 'verticalLine',
    afterDraw: (chart) => {
      if (epdDetectedTime > 0) {
        const ctx = chart.ctx;
        const xAxis = chart.scales.x;
        const yAxis = chart.scales['y-depth'];
        
        let matchIndex = -1;
        let minDiff = 999;
        chart.data.labels.forEach((l, index) => {
           const diff = Math.abs(parseFloat(l) - epdDetectedTime);
           if (diff < minDiff) {
             minDiff = diff;
             matchIndex = index;
           }
        });
        
        if (matchIndex >= 0 && minDiff < 2.0) { // Close enough to be visible on chart
          const xPixel = xAxis.getPixelForTick(matchIndex);
          ctx.save();
          ctx.beginPath();
          ctx.moveTo(xPixel, yAxis.top);
          ctx.lineTo(xPixel, yAxis.bottom);
          ctx.lineWidth = 2;
          ctx.strokeStyle = '#10b981';
          ctx.setLineDash([5, 5]);
          ctx.stroke();
          
          ctx.fillStyle = '#10b981';
          ctx.font = '10px Inter';
          ctx.fillText('EPD', xPixel + 5, yAxis.top + 10);
          ctx.restore();
        }
      }
    }
  };

  const trendChart = new Chart(ctx, {
    type: 'line',
    plugins: [verticalLinePlugin],
    data: {
      labels: [],
      datasets: [
        {
          label: 'Etch Depth (nm)',
          data: [],
          borderColor: '#06b6d4',
          backgroundColor: 'rgba(6, 182, 212, 0.1)',
          borderWidth: 2,
          yAxisID: 'y-depth',
          tension: 0.1,
          pointRadius: 0
        },
        {
          label: 'OES Intensity (a.u.)',
          data: [],
          borderColor: '#8b5cf6',
          backgroundColor: 'transparent',
          borderWidth: 2,
          yAxisID: 'y-oes',
          tension: 0.1,
          pointRadius: 0
        },
        {
          label: 'Pressure (mTorr)',
          data: [],
          borderColor: '#f59e0b',
          backgroundColor: 'transparent',
          borderWidth: 1.5,
          borderDash: [4, 4],
          yAxisID: 'y-press',
          tension: 0.1,
          pointRadius: 0
        }
      ]
    },
    options: {
      animation: false, // Disable animation for high-speed updates
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        x: {
          grid: { color: 'rgba(255,255,255,0.03)' },
          title: { display: true, text: 'Process Time (s)', font: { size: 9 } }
        },
        'y-depth': {
          type: 'linear',
          position: 'left',
          min: 0,
          max: 100,
          grid: { color: 'rgba(255,255,255,0.05)' },
          title: { display: true, text: 'Depth (nm)', color: '#06b6d4' }
        },
        'y-oes': {
          type: 'linear',
          position: 'right',
          min: 0,
          max: 1.2,
          grid: { drawOnChartArea: false },
          title: { display: true, text: 'OES / Pressure', color: '#8b5cf6' }
        },
        'y-press': {
          type: 'linear',
          position: 'right',
          min: 0,
          max: 40,
          display: false,
          grid: { drawOnChartArea: false }
        }
      },
      plugins: {
        legend: {
          position: 'top',
          labels: { boxWidth: 12, padding: 8, font: { size: 9 } }
        }
      }
    }
  });

  // --- Helper Functions ---
  
  // Format real-time timestamp for UI Console
  function getTimestamp() {
    const now = new Date();
    return now.toTimeString().split(' ')[0];
  }

  // Format simulated timestamp for CSV data (adds simulated seconds to start time)
  function getSimulatedTimestamp(elapsedSeconds) {
    const simDate = new Date(simulationStartTime + (elapsedSeconds * 1000));
    const pad = (num, size) => num.toString().padStart(size, '0');
    const hh = pad(simDate.getHours(), 2);
    const mm = pad(simDate.getMinutes(), 2);
    const ss = pad(simDate.getSeconds(), 2);
    const ms = pad(simDate.getMilliseconds(), 3);
    return `${hh}:${mm}:${ss}.${ms}`;
  }

  // Log to UI Console & Alarm History
  function writeLog(message, type = 'info') {
    const timestamp = getTimestamp();
    
    // UI Console
    const logLine = document.createElement('div');
    logLine.className = `log-line log-${type}`;
    logLine.innerHTML = `<span class="log-time">[${timestamp}]</span> <span class="log-msg">${message}</span>`;
    consoleLog.appendChild(logLine);
    consoleLog.scrollTop = consoleLog.scrollHeight;
    
    // Alarm History
    let alarmLevel = "INFO";
    if (type === 'warning') alarmLevel = "WARNING";
    if (type === 'fault' || type === 'error') {
      alarmLevel = message.includes("CRITICAL") ? "CRITICAL" : "ERROR";
    }
    
    alarmHistoryData.push({
      time: timestamp,
      level: alarmLevel,
      message: message
    });
    
    if (alarmHistoryData.length > 200) {
      alarmHistoryData.shift(); // Remove oldest
    }
    
    renderAlarmTable();
  }

  // Render Alarm Table
  function renderAlarmTable() {
    const showInfo = elFilterInfo.checked;
    const showWarning = elFilterWarning.checked;
    const showError = elFilterError.checked;
    const showCritical = elFilterCritical.checked;
    
    const filteredAlarms = alarmHistoryData.filter(alarm => {
      if (alarm.level === "INFO" && !showInfo) return false;
      if (alarm.level === "WARNING" && !showWarning) return false;
      if (alarm.level === "ERROR" && !showError) return false;
      if (alarm.level === "CRITICAL" && !showCritical) return false;
      return true;
    });
    
    elAlarmCountBadge.textContent = `Total: ${filteredAlarms.length}`;
    
    let html = "";
    // Display newest first by reversing the filtered array
    for (let i = filteredAlarms.length - 1; i >= 0; i--) {
      const alarm = filteredAlarms[i];
      html += `
        <tr>
          <td>${alarm.time}</td>
          <td class="alarm-level-${alarm.level}">${alarm.level}</td>
          <td>${alarm.message}</td>
        </tr>
      `;
    }
    elAlarmTableBody.innerHTML = html;
  }

  // Clear UI Console
  btnClearConsole.addEventListener('click', () => {
    consoleLog.innerHTML = '';
    writeLog("Console cleared.", "info");
  });

  // Alarm Panel Events
  btnClearAlarms.addEventListener('click', () => {
    alarmHistoryData = [];
    renderAlarmTable();
    writeLog("Alarm History cleared.", "info");
  });
  
  elFilterInfo.addEventListener('change', renderAlarmTable);
  elFilterWarning.addEventListener('change', renderAlarmTable);
  elFilterError.addEventListener('change', renderAlarmTable);
  elFilterCritical.addEventListener('change', renderAlarmTable);

  // Recipe Editor Functions
  function renderRecipeEditor() {
    elRecipeTableBody.innerHTML = '';
    
    RECIPE_STEPS.forEach((step, index) => {
      const tr = document.createElement('tr');
      tr.className = 'recipe-row';
      tr.id = `step-row-${index + 1}`;
      if (index === selectedRecipeIndex) {
        tr.classList.add('selected-row');
      }
      
      // Update step numbers sequentially just in case
      step.stepNum = index + 1;
      
      tr.innerHTML = `
        <td class="step-num">${step.stepNum}</td>
        <td><input type="text" class="recipe-input text-left" data-index="${index}" data-field="name" value="${step.name}"></td>
        <td><input type="number" class="recipe-input" data-index="${index}" data-field="duration" value="${step.duration}" min="1"></td>
        <td><input type="number" class="recipe-input" data-index="${index}" data-field="rfSource" value="${step.rfSource}" min="0"></td>
        <td><input type="number" class="recipe-input" data-index="${index}" data-field="rfBias" value="${step.rfBias}" min="0"></td>
        <td><input type="number" step="0.1" class="recipe-input" data-index="${index}" data-field="pressure" value="${step.pressure}" min="0"></td>
        <td><input type="number" class="recipe-input" data-index="${index}" data-field="cl2" value="${step.cl2}" min="0"></td>
        <td><input type="number" class="recipe-input" data-index="${index}" data-field="hbr" value="${step.hbr}" min="0"></td>
        <td><input type="number" class="recipe-input" data-index="${index}" data-field="ar" value="${step.ar}" min="0"></td>
      `;
      
      // Select row on click (but not if clicking input to focus)
      tr.addEventListener('click', (e) => {
        selectedRecipeIndex = index;
        renderRecipeEditor();
        highlightRecipeStep(currentStepIndex); // Restore active highlight if running
      });
      
      elRecipeTableBody.appendChild(tr);
    });

    // Attach input change listeners
    document.querySelectorAll('.recipe-input').forEach(input => {
      input.addEventListener('change', (e) => {
        const idx = parseInt(e.target.getAttribute('data-index'));
        const field = e.target.getAttribute('data-field');
        let val = e.target.value;
        if (field !== 'name') val = parseFloat(val) || 0;
        
        RECIPE_STEPS[idx][field] = val;
        writeLog(`Recipe step ${idx+1} [${field}] changed to ${val}`, "info");
      });
      
      // Stop row click from stealing focus immediately
      input.addEventListener('click', (e) => {
        e.stopPropagation();
        selectedRecipeIndex = parseInt(e.target.getAttribute('data-index'));
        document.querySelectorAll('.recipe-row').forEach((r, i) => {
          if (i === selectedRecipeIndex) r.classList.add('selected-row');
          else r.classList.remove('selected-row');
        });
      });
    });
  }

  // Highlight Recipe Step row
  function highlightRecipeStep(stepIndex) {
    const rows = document.querySelectorAll('.recipe-row');
    rows.forEach(r => r.classList.remove('active-step'));
    
    if (stepIndex >= 0 && stepIndex < RECIPE_STEPS.length) {
      const activeRow = document.getElementById(`step-row-${stepIndex + 1}`);
      if (activeRow) activeRow.classList.add('active-step');
    }
  }

  // Recipe Toolbar Events
  btnAddStep.addEventListener('click', () => {
    RECIPE_STEPS.push({
      stepNum: RECIPE_STEPS.length + 1,
      name: "New Step", duration: 10, rfSource: 0, rfBias: 0, pressure: 20.0,
      cl2: 0, hbr: 0, ar: 0, targetEtchRate: 0, targetUniformity: 90, targetSelectivity: 0, plasmaColor: "url(#plasma-etch)"
    });
    renderRecipeEditor();
    writeLog("Added new recipe step.", "info");
  });

  btnDelStep.addEventListener('click', () => {
    if (selectedRecipeIndex >= 0 && selectedRecipeIndex < RECIPE_STEPS.length) {
      RECIPE_STEPS.splice(selectedRecipeIndex, 1);
      selectedRecipeIndex = -1;
      renderRecipeEditor();
      writeLog("Deleted recipe step.", "warning");
    }
  });

  btnDupStep.addEventListener('click', () => {
    if (selectedRecipeIndex >= 0 && selectedRecipeIndex < RECIPE_STEPS.length) {
      const clone = { ...RECIPE_STEPS[selectedRecipeIndex] };
      RECIPE_STEPS.splice(selectedRecipeIndex + 1, 0, clone);
      renderRecipeEditor();
      writeLog("Duplicated recipe step.", "info");
    }
  });

  btnUpStep.addEventListener('click', () => {
    if (selectedRecipeIndex > 0) {
      const temp = RECIPE_STEPS[selectedRecipeIndex - 1];
      RECIPE_STEPS[selectedRecipeIndex - 1] = RECIPE_STEPS[selectedRecipeIndex];
      RECIPE_STEPS[selectedRecipeIndex] = temp;
      selectedRecipeIndex--;
      renderRecipeEditor();
    }
  });

  btnDownStep.addEventListener('click', () => {
    if (selectedRecipeIndex >= 0 && selectedRecipeIndex < RECIPE_STEPS.length - 1) {
      const temp = RECIPE_STEPS[selectedRecipeIndex + 1];
      RECIPE_STEPS[selectedRecipeIndex + 1] = RECIPE_STEPS[selectedRecipeIndex];
      RECIPE_STEPS[selectedRecipeIndex] = temp;
      selectedRecipeIndex++;
      renderRecipeEditor();
    }
  });

  btnSaveLocal.addEventListener('click', () => {
    localStorage.setItem('sym3_recipe', JSON.stringify(RECIPE_STEPS));
    writeLog("Recipe saved to local memory.", "success");
  });

  btnLoadLocal.addEventListener('click', () => {
    const saved = localStorage.getItem('sym3_recipe');
    if (saved) {
      try {
        RECIPE_STEPS = JSON.parse(saved);
        renderRecipeEditor();
        writeLog("Recipe loaded from local memory.", "success");
      } catch (e) {
        writeLog("Failed to load recipe.", "error");
      }
    } else {
      writeLog("No recipe found in memory.", "warning");
    }
  });

  btnExportJson.addEventListener('click', () => {
    const dataStr = "data:text/json;charset=utf-8," + encodeURIComponent(JSON.stringify(RECIPE_STEPS, null, 2));
    const dlAnchorElem = document.createElement('a');
    dlAnchorElem.setAttribute("href", dataStr);
    dlAnchorElem.setAttribute("download", "recipe_sym3.json");
    dlAnchorElem.click();
    writeLog("Exported recipe as JSON.", "success");
  });

  btnImportJson.addEventListener('click', () => {
    inputJsonUpload.click();
  });

  inputJsonUpload.addEventListener('change', (e) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = function(evt) {
        try {
          RECIPE_STEPS = JSON.parse(evt.target.result);
          renderRecipeEditor();
          writeLog(`Imported recipe from ${file.name}.`, "success");
        } catch(err) {
          writeLog("Invalid JSON format in file.", "error");
        }
      };
      reader.readAsText(file);
    }
  });

  // Initial render
  renderRecipeEditor();

  // Update Radial Uniformity Gauge
  function updateUniformityGauge(val) {
    const clampVal = Math.max(0, Math.min(100, val));
    const offset = 251.2 - (clampVal / 100) * 251.2;
    elUniformityGauge.style.strokeDashoffset = offset;
    elValUniformity.textContent = `${val.toFixed(1)}%`;
    
    if (val >= 95.0) {
      elUniformityGauge.style.stroke = 'var(--color-green)';
      elUniformityStatus.textContent = "NORMAL";
      elUniformityStatus.className = "tel-sub highlight-green";
    } else if (val >= 90.0) {
      elUniformityGauge.style.stroke = 'var(--color-orange)';
      elUniformityStatus.textContent = "WARNING";
      elUniformityStatus.className = "tel-sub highlight-orange";
    } else {
      elUniformityGauge.style.stroke = 'var(--color-red)';
      elUniformityStatus.textContent = "FAULT";
      elUniformityStatus.className = "tel-sub highlight-red";
    }
  }

  // Update UI Elements with active telemetry
  function updateUI() {
    elStepNumber.textContent = `${currentStepIndex + 1} / ${RECIPE_STEPS.length}`;
    
    // Process State Badge
    elProcessState.className = "value state-badge";
    if (isRunning) {
      if (isPaused) {
        elProcessState.classList.add('state-paused');
        elProcessState.textContent = "PAUSED";
      } else {
        elProcessState.classList.add('state-executing');
        elProcessState.textContent = "EXECUTING";
      }
    } else if (telemetry.status === "COMPLETE") {
      elProcessState.classList.add('state-complete');
      elProcessState.textContent = "COMPLETE";
    } else if (telemetry.status === "FAULT") {
      elProcessState.classList.add('state-fault');
      elProcessState.textContent = "ABORTED";
    } else if (currentStepIndex === -1) {
      elProcessState.classList.add('state-idle');
      elProcessState.textContent = "IDLE";
    } else {
      elProcessState.classList.add('state-stopped');
      elProcessState.textContent = "STOPPED";
    }

    // Health State Badge
    elHealthState.className = "value health-badge";
    if (telemetry.status === "NORMAL" || telemetry.status === "COMPLETE") {
      elHealthState.classList.add('health-normal');
      elHealthState.textContent = "NORMAL";
    } else if (telemetry.status === "WARNING") {
      elHealthState.classList.add('health-warning');
      elHealthState.textContent = "WARNING";
    } else if (telemetry.status === "FAULT") {
      elHealthState.classList.add('health-fault');
      elHealthState.textContent = "FAULT";
    }

    // Chamber mimic overlays
    elTelRefl.textContent = `${telemetry.rfReflection.toFixed(1)} W`;
    elTelPress.textContent = `${telemetry.pressure.toFixed(2)} mTorr`;
    elTelTemp.textContent = `${telemetry.temp.toFixed(1)} °C`;

    // Output Cards
    elValEtchRate.textContent = telemetry.etchRate.toFixed(2);
    elValEtchDepth.textContent = telemetry.etchDepth.toFixed(2);
    elValSelectivity.textContent = telemetry.selectivity.toFixed(1);
    updateUniformityGauge(telemetry.uniformity);

    // Final result Quality Badge
    elFinalResultBadge.className = "result-badge";
    if (telemetry.result === "OK") {
      elFinalResultBadge.classList.add('badge-ok');
      elFinalResultBadge.textContent = "OK";
    } else {
      elFinalResultBadge.classList.add('badge-ng');
      elFinalResultBadge.textContent = "NG";
    }
    
    // EPD UI Update
    elValEpdOes.textContent = telemetry.oesIntensity.toFixed(3);
    elValEpdThreshold.textContent = epdThreshold.toFixed(3);
    
    if (epdState === "WAITING") {
      elEpdStatus.textContent = "WAITING";
      elEpdStatus.className = "epd-status-badge badge-waiting";
      elEpdProgressBar.style.width = "0%";
      elEpdProgressText.textContent = "0%";
      elValEpdTime.textContent = "-- s";
    } else if (epdState === "TRACKING") {
      elEpdStatus.textContent = "TRACKING";
      elEpdStatus.className = "epd-status-badge badge-tracking";
      
      const startOes = 0.82; // approximate nominal before drop
      let progress = ((startOes - telemetry.oesIntensity) / (startOes - epdThreshold)) * 100;
      progress = Math.max(0, Math.min(100, progress));
      elEpdProgressBar.style.width = `${progress}%`;
      elEpdProgressText.textContent = `${progress.toFixed(1)}%`;
      
      const depthRemaining = Math.max(0.1, THIN_FILM_LIMIT - telemetry.etchDepth);
      const erPerSec = telemetry.etchRate / 60.0;
      const timeRemaining = (erPerSec > 0) ? (depthRemaining / erPerSec).toFixed(1) : "--";
      elValEpdTime.textContent = `${timeRemaining} s`;
    } else if (epdState === "DETECTED") {
      elEpdStatus.textContent = "DETECTED";
      elEpdStatus.className = "epd-status-badge badge-detected";
      elEpdProgressBar.style.width = "100%";
      elEpdProgressText.textContent = "100%";
      elValEpdTime.textContent = "0.0 s";
    }

    // Chamber Diagram Animation Controls
    if (isRunning && !isPaused) {
      if (telemetry.rfSource > 0) {
        svgCoilLeft.className.baseVal = "coil-active";
        svgCoilRight.className.baseVal = "coil-active";
        svgPlasmaGlow.style.opacity = "0.75";
        svgPlasmaGlow.className.baseVal = "plasma-on";
        
        if (currentStepIndex >= 0 && currentStepIndex < RECIPE_STEPS.length) {
          svgPlasmaGlow.setAttribute('fill', RECIPE_STEPS[currentStepIndex].plasmaColor);
        }
        if (telemetry.status === "FAULT") {
          svgPlasmaGlow.setAttribute('fill', 'url(#plasma-fault)');
        }
      } else {
        svgCoilLeft.className.baseVal = "coil-inactive";
        svgCoilRight.className.baseVal = "coil-inactive";
        svgPlasmaGlow.style.opacity = "0";
        svgPlasmaGlow.className.baseVal = "";
      }

      svgBiasGlow.style.opacity = (telemetry.rfBias > 0) ? "0.8" : "0";
      svgTurboFan.classList.add('pump-running');
      
      const angle = 90 - ((telemetry.pressure - 5) / 35) * 85;
      svgValveFlap.setAttribute('transform', `rotate(${angle})`);

      flowCl2.classList.toggle('gas-active', telemetry.cl2 > 0);
      flowHbr.classList.toggle('gas-active', telemetry.hbr > 0);
      flowAr.classList.toggle('gas-active', telemetry.ar > 0);
      flowO2.classList.toggle('gas-active', isRunning);
      flowMain.classList.toggle('gas-active', (telemetry.cl2 + telemetry.hbr + telemetry.ar) > 0);
      
      svgWafer.setAttribute('fill', `rgb(${100 + Math.max(0, telemetry.temp - 40) * 2.5}, 116, 139)`);

    } else {
      svgCoilLeft.className.baseVal = "coil-inactive";
      svgCoilRight.className.baseVal = "coil-inactive";
      svgPlasmaGlow.style.opacity = "0";
      svgPlasmaGlow.className.baseVal = "";
      svgBiasGlow.style.opacity = "0";
      svgTurboFan.classList.remove('pump-running');
      svgValveFlap.setAttribute('transform', `rotate(45)`);
      
      flowCl2.classList.remove('gas-active');
      flowHbr.classList.remove('gas-active');
      flowAr.classList.remove('gas-active');
      flowO2.classList.remove('gas-active');
      flowMain.classList.remove('gas-active');
    }
  }

  // --- Simulation Mathematics and Logic ---

  function runSimulationTick() {
    if (!isRunning || isPaused) return;

    let logMessages = [];
    
    // Execute multiple sub-steps per UI tick to reach 10,000 points quickly
    for (let i = 0; i < SUB_STEPS_PER_TICK; i++) {
      if (logHistory.length >= MAX_DATA_POINTS || currentStepIndex >= RECIPE_STEPS.length) {
        break; // End condition met
      }

      totalTimeElapsed += DT;
      stepTimeElapsed += DT;
      anomalySteps++;

      const activeStep = RECIPE_STEPS[currentStepIndex];

      processScenarioEvents(logMessages);
      updateInputs(activeStep);
      calculateOutputs(activeStep);
      evaluateEPD(logMessages);

      // Append to Data Log array
      logHistory.push({
        timestamp: getSimulatedTimestamp(totalTimeElapsed),
        equipment_id: EQ_ID,
        rf_source_power: Math.round(telemetry.rfSource),
        rf_bias_power: Math.round(telemetry.rfBias),
        pressure: parseFloat(telemetry.pressure.toFixed(2)),
        cl2_flow: Math.round(telemetry.cl2),
        hbr_flow: Math.round(telemetry.hbr),
        etch_rate: parseFloat(telemetry.etchRate.toFixed(3)),
        etch_depth: parseFloat(telemetry.etchDepth.toFixed(3)),
        oes_intensity: parseFloat(telemetry.oesIntensity.toFixed(4)),
        status: telemetry.status,
        result: telemetry.result
      });

      // Check for faults
      if (telemetry.status === "FAULT" && !logMessages.some(m => m.msg.includes("CRITICAL FAULT"))) {
        logMessages.push({msg: `CRITICAL FAULT DETECTED! INTERRUPTING SIMULATION...`, type: 'fault'});
        break; 
      }

      // Check step advancement
      if (!overrideMode && stepTimeElapsed >= activeStep.duration) {
        currentStepIndex++;
        stepTimeElapsed = 0;
        if (currentStepIndex < RECIPE_STEPS.length) {
          const nextStep = RECIPE_STEPS[currentStepIndex];
          logMessages.push({msg: `Recipe Step ${nextStep.stepNum} (${nextStep.name}) Started.`, type: 'info'});
        } else {
          break; // Recipe complete
        }
      }
    } // End of sub-steps loop

    // Flush DOM logs
    logMessages.forEach(log => writeLog(log.msg, log.type));

    // Update UI (once per batch for performance)
    highlightRecipeStep(currentStepIndex);
    updateChart(totalTimeElapsed);
    updateUI();

    // End condition handling
    if (telemetry.status === "FAULT") {
      stopSimulation();
      return;
    }

    if (logHistory.length >= MAX_DATA_POINTS || currentStepIndex >= RECIPE_STEPS.length) {
      telemetry.status = "COMPLETE";
      telemetry.result = checkFinalQuality() ? "OK" : "NG";
      stopSimulation();
      updateUI();
      writeLog(`Process sequence completed successfully.`, 'success');
      writeLog(`Generated exactly ${logHistory.length} data points. Result: ${telemetry.result}`, telemetry.result === 'OK' ? 'success' : 'warning');
    }
  }

  function processScenarioEvents(logMsgs) {
    const selectedScenario = selScenario.value;
    
    // Auto-fault injections during Step 3 (Main Etch)
    if (!overrideMode && currentStepIndex === 2) {
      if (selectedScenario === 'gas_low') {
        if (stepTimeElapsed >= 15.0 && activeAnomaly !== 'gas_low') {
          activeAnomaly = 'gas_low';
          anomalySteps = 0;
          logMsgs.push({msg: `SCENARIO TRIGGERED: Gas Flow Low Anomaly. Injecting Cl2 leak.`, type: 'warning'});
        }
      } else if (selectedScenario === 'rf_fault') {
        if (stepTimeElapsed >= 12.0 && activeAnomaly !== 'rf_fault') {
          activeAnomaly = 'rf_fault';
          anomalySteps = 0;
          logMsgs.push({msg: `SCENARIO TRIGGERED: RF Instability Fault. Matcher failing.`, type: 'fault'});
        }
      }
    }

    // Periodic warnings every ~1000 steps (10 seconds process time)
    if (activeAnomaly === 'gas_low' && anomalySteps % 1000 === 0) {
      logMsgs.push({msg: `[ALARM] WARNING - Cl2 Flow Low (95 sccm). Pressure fluctuation: ${telemetry.pressure.toFixed(2)}mTorr`, type: 'warning'});
    }
    if (activeAnomaly === 'rf_fault' && anomalySteps % 800 === 0) {
      logMsgs.push({msg: `[ALARM] FAULT - RF Reflection surge (${telemetry.rfReflection.toFixed(1)}W). Uniformity degrading.`, type: 'fault'});
    }
  }

  function updateInputs(activeStep) {
    let srcRF = activeStep.rfSource;
    let biasRF = activeStep.rfBias;
    let pressChamber = activeStep.pressure;
    let flowCl2Act = activeStep.cl2;
    let flowHbrAct = activeStep.hbr;
    let flowArAct = activeStep.ar;
    let tempChamber = telemetry.temp;

    if (overrideMode) {
      srcRF = manualInputs.rfSource;
      biasRF = manualInputs.rfBias;
      pressChamber = manualInputs.pressure;
      flowCl2Act = manualInputs.cl2;
      flowHbrAct = manualInputs.hbr;
      tempChamber = manualInputs.temp;
    } else {
      tempChamber += (65.0 - tempChamber) * 0.005; // gradually stabilize temp
    }

    if (activeAnomaly === 'gas_low') {
      flowCl2Act = 95;
      pressChamber = 20.0 + 3.2 * Math.sin(anomalySteps * 0.008) + (Math.random() - 0.5) * 0.5;
    } else if (activeAnomaly === 'rf_fault') {
      biasRF = biasRF + Math.sin(anomalySteps * 0.015) * 28 + (Math.random() - 0.5) * 8;
      biasRF = Math.max(50, Math.min(250, biasRF));
    }

    telemetry.rfSource = srcRF;
    telemetry.rfBias = biasRF;
    telemetry.pressure = pressChamber;
    telemetry.cl2 = flowCl2Act;
    telemetry.hbr = flowHbrAct;
    telemetry.ar = flowArAct;
    telemetry.temp = tempChamber + (Math.random() - 0.5) * 0.1; 
  }

  function calculateOutputs(activeStep) {
    const stepNum = activeStep.stepNum;
    const noise = (Math.random() - 0.5) * 0.3;

    // A. RF Reflection
    if (activeAnomaly === 'rf_fault') {
      telemetry.rfReflection = 28.5 + (Math.random() - 0.5) * 4.5;
    } else {
      telemetry.rfReflection = (telemetry.rfBias * 0.01) + (Math.random() * 0.8);
    }

    // B. Etch Rate calculation
    if (stepNum === 1 || telemetry.rfSource < 200 || telemetry.rfBias < 20) {
      telemetry.etchRate = 0.0;
    } else {
      const rfSrcFactor = telemetry.rfSource / activeStep.rfSource;
      const pressFactor = Math.sqrt(telemetry.pressure / activeStep.pressure);
      const activeTotalGas = (telemetry.cl2 + 0.6 * telemetry.hbr);
      const baseTotalGas = (activeStep.cl2 + 0.6 * activeStep.hbr);
      const gasFactor = baseTotalGas > 0 ? (activeTotalGas / baseTotalGas) : 1.0;
      
      let calculatedEr = activeStep.targetEtchRate * rfSrcFactor * pressFactor * gasFactor;
      
      if (activeAnomaly === 'gas_low') {
        calculatedEr += (Math.random() - 0.5) * 3.5;
      } else {
        calculatedEr += noise;
      }
      telemetry.etchRate = Math.max(0, calculatedEr);
    }

    // C. Etch Depth increment (ER is nm/min, so we add ER/60 per second, meaning ER/60 * DT per step)
    telemetry.etchDepth += (telemetry.etchRate / 60.0) * DT;

    // D. OES Intensity
    if (stepNum === 1 || telemetry.rfSource < 200) {
      telemetry.oesIntensity = 0.08 + (Math.random() * 0.02);
    } else if (stepNum === 2) {
      telemetry.oesIntensity = 0.42 + noise * 0.05;
    } else if (stepNum === 3) {
      if (telemetry.etchDepth < THIN_FILM_LIMIT) {
        telemetry.oesIntensity = 0.82 + (Math.random() - 0.5) * 0.03;
      } else {
        const transitionProgress = telemetry.etchDepth - THIN_FILM_LIMIT;
        telemetry.oesIntensity = 0.15 + 0.67 * Math.exp(-transitionProgress / 1.5) + noise * 0.04;
      }
    } else {
      telemetry.oesIntensity = 0.12 + noise * 0.02;
    }

    if (activeAnomaly === 'rf_fault') {
      telemetry.oesIntensity += (Math.random() - 0.5) * 0.18;
    }
    telemetry.oesIntensity = Math.max(0, telemetry.oesIntensity);

    // E. Uniformity calculation
    let baseUniformity = activeStep.targetUniformity;
    let uDegradation = 0.0;
    
    const pressDev = Math.abs(telemetry.pressure - activeStep.pressure);
    if (pressDev > 2.0) {
      uDegradation += (pressDev - 2.0) * 1.8;
    }
    
    const tempDev = Math.abs(telemetry.temp - 65.0);
    if (tempDev > 5.0) {
      uDegradation += (tempDev - 5.0) * 0.6;
    }

    if (activeAnomaly === 'rf_fault') {
      telemetry.uniformity = 86.8 + (Math.random() - 0.5) * 1.5;
    } else {
      telemetry.uniformity = Math.max(70, Math.min(100, baseUniformity - uDegradation + (Math.random() - 0.5) * 0.2));
    }

    // F. Selectivity
    if (telemetry.cl2 > 0) {
      const hbrRatio = telemetry.hbr / telemetry.cl2;
      const calculatedSelectivity = (hbrRatio * 29.3) * (1.0 - (telemetry.temp - 65.0) / 180.0);
      telemetry.selectivity = Math.max(0, calculatedSelectivity + noise * 0.2);
    } else {
      telemetry.selectivity = 0.0;
    }

    // G. Status and Quality
    evaluateStatusAndResult(activeStep);
  }

  function evaluateEPD(logMsgs) {
    if (epdState === "DETECTED") return;

    if (currentStepIndex === 2 && stepTimeElapsed > 5.0) {
      if (epdState === "WAITING") {
        epdState = "TRACKING";
      }

      if (epdState === "TRACKING" && telemetry.oesIntensity <= epdThreshold) {
        epdState = "DETECTED";
        epdDetectedTime = totalTimeElapsed;
        
        logMsgs.push({msg: `[ENDPOINT DETECTED] OES Intensity dropped below ${epdThreshold.toFixed(3)}.`, type: 'success'});
        
        // Show Overlay Popup directly via DOM (since this happens inside fast loop, we push to UI thread safely)
        setTimeout(() => {
          elEpdPopupOverlay.classList.remove('hidden');
          svgPlasmaGlow.classList.add('plasma-epd-green');
          
          setTimeout(() => {
            elEpdPopupOverlay.classList.add('hidden');
            svgPlasmaGlow.classList.remove('plasma-epd-green');
          }, 3000);
        }, 0);
      }
    }
  }

  function evaluateStatusAndResult(activeStep) {
    let status = "NORMAL";
    
    const isStep34 = (currentStepIndex === 2 || currentStepIndex === 3);
    const hasCl2Warning = isStep34 && (telemetry.cl2 < 100);
    const hasUniformityWarning = (telemetry.uniformity >= 90.0 && telemetry.uniformity < 95.0);
    const hasPressureWarning = Math.abs(telemetry.pressure - activeStep.pressure) >= 3.0;

    if (hasCl2Warning || hasUniformityWarning || hasPressureWarning) {
      status = "WARNING";
    }

    const hasReflectionFault = (telemetry.rfReflection >= 20.0);
    const hasUniformityFault = (telemetry.uniformity < 90.0);

    if (hasReflectionFault || hasUniformityFault) {
      status = "FAULT";
    }

    telemetry.status = status;
    
    if (currentStepIndex > 0) {
      let stepOk = true;
      if (telemetry.uniformity < 95.0) stepOk = false;
      if (currentStepIndex >= 2 && telemetry.selectivity < 20.0) stepOk = false;
      
      const targetEr = activeStep.targetEtchRate;
      if (targetEr > 0) {
        const erDev = Math.abs(telemetry.etchRate - targetEr) / targetEr;
        if (erDev > 0.05) stepOk = false;
      }
      telemetry.result = stepOk ? "OK" : "NG";
    } else {
      telemetry.result = "OK"; 
    }
  }

  function checkFinalQuality() {
    const mainEtchLogs = logHistory.filter(l => l.cl2_flow === 120 || l.status === "WARNING" || l.status === "FAULT");
    if (mainEtchLogs.some(l => l.status === "FAULT")) return false;
    
    const avgUniformity = mainEtchLogs.reduce((acc, curr) => acc + curr.uniformity, 0) / (mainEtchLogs.length || 1);
    if (avgUniformity < 95.0) return false;

    return true;
  }

  // --- Chart Control Methods ---
  function updateChart(time) {
    trendChart.data.labels.push(time.toFixed(1));
    trendChart.data.datasets[0].data.push(telemetry.etchDepth);
    trendChart.data.datasets[1].data.push(telemetry.oesIntensity);
    trendChart.data.datasets[2].data.push(telemetry.pressure);
    
    if (trendChart.data.labels.length > 100) {
      trendChart.data.labels.shift();
      trendChart.data.datasets[0].data.shift();
      trendChart.data.datasets[1].data.shift();
      trendChart.data.datasets[2].data.shift();
    }
    trendChart.update('none'); 
  }

  function clearChart() {
    trendChart.data.labels = [];
    trendChart.data.datasets.forEach(dataset => dataset.data = []);
    trendChart.update('none');
  }

  // --- Main Control Operations ---

  function startSimulation() {
    if (isRunning && !isPaused) return;

    if (!isPaused) {
      writeLog(`Starting process sequence. Accelerated generation mode active (100x).`, 'info');
      simulationStartTime = Date.now();
      
      if (overrideMode) {
        currentStepIndex = 2; 
        writeLog(`Manual Override Mode active. Manual inputs injected.`, 'warning');
      } else {
        currentStepIndex = 0;
        writeLog(`Recipe Step 1 (Strike) Started.`, 'info');
      }
      
      highlightRecipeStep(currentStepIndex);
      elCommState.textContent = "ONLINE";
      elCommState.className = "value text-success";
    } else {
      writeLog(`Resuming process execution.`, 'info');
    }

    isRunning = true;
    isPaused = false;
    
    btnStart.disabled = true;
    btnPause.disabled = false;
    btnStop.disabled = false;
    
    simTimer = setInterval(runSimulationTick, UI_TICK_MS);
    updateUI();
  }

  function pauseSimulation() {
    if (!isRunning || isPaused) return;
    
    isPaused = true;
    clearInterval(simTimer);
    writeLog(`Process PAUSED by operator. Safe conditions maintained.`, 'warning');
    
    btnStart.disabled = false;
    btnPause.disabled = true;
    updateUI();
  }

  function stopSimulation() {
    if (!isRunning) return;
    
    isRunning = false;
    isPaused = false;
    clearInterval(simTimer);
    
    writeLog(`Process STOPPED. Powering down chamber systems.`, 'info');
    
    btnStart.disabled = false;
    btnPause.disabled = true;
    btnStop.disabled = true;
    
    elCommState.textContent = "REMOTE";
    elCommState.className = "value text-muted";
    
    highlightRecipeStep(-1);
    updateUI();
  }

  function abortSimulation() {
    isRunning = false;
    isPaused = false;
    clearInterval(simTimer);
    
    telemetry.status = "FAULT";
    telemetry.result = "NG";
    telemetry.rfSource = 0;
    telemetry.rfBias = 0;
    telemetry.cl2 = 0;
    telemetry.hbr = 0;
    telemetry.ar = 0;
    telemetry.rfReflection = 0;
    
    writeLog(`CRITICAL ABORT ACTION TRIGGERED BY OPERATOR!`, 'fault');
    writeLog(`Chamber vacuum isolation valves CLOSED.`, 'fault');
    
    btnStart.disabled = false;
    btnPause.disabled = true;
    btnStop.disabled = true;
    
    elCommState.textContent = "LOCAL-LOCK";
    elCommState.className = "value text-danger";
    
    highlightRecipeStep(-1);
    updateUI();
  }

  function resetSimulation() {
    stopSimulation();
    clearChart();
    
    currentStepIndex = -1;
    stepTimeElapsed = 0;
    totalTimeElapsed = 0;
    logHistory = [];
    activeAnomaly = null;
    anomalySteps = 0;
    simulationStartTime = 0;
    
    epdState = "WAITING";
    epdDetectedTime = -1;
    
    telemetry = {
      rfSource: 0,
      rfBias: 0,
      pressure: 0.0,
      cl2: 0,
      hbr: 0,
      ar: 0,
      temp: 65.0,
      etchRate: 0.0,
      etchDepth: 0.0,
      oesIntensity: 0.0,
      uniformity: 100.0,
      selectivity: 0.0,
      rfReflection: 0.0,
      status: "NORMAL",
      result: "OK"
    };

    manualInputs = {
      rfSource: 900,
      rfBias: 150,
      pressure: 20.0,
      cl2: 120,
      hbr: 90,
      temp: 65.0
    };
    
    inRfSource.value = 900;
    valRfSource.textContent = "900";
    inRfBias.value = 150;
    valRfBias.textContent = "150";
    inCl2Flow.value = 120;
    valCl2Flow.textContent = "120";
    inHbrFlow.value = 90;
    valHbrFlow.textContent = "90";
    inPressure.value = 20.0;
    inTemp.value = 65.0;

    writeLog(`Simulator system reset. Chamber state IDLE.`, 'info');
    updateUI();
  }

  function saveRecipe() {
    writeLog(`Saving GAA_MAIN_ETCH_R3 parameters to equipment memory.`, 'info');
    writeLog(`Recipe configuration saved successfully.`, 'success');
  }

  function exportCSV() {
    if (logHistory.length === 0) {
      writeLog(`Cannot export CSV: No data points logged. Start simulation first.`, 'warning');
      return;
    }
    
    let csvContent = "timestamp,equipment_id,rf_source_power,rf_bias_power,pressure,cl2_flow,hbr_flow,etch_rate,etch_depth,oes_intensity,status,result\n";
    
    logHistory.forEach(row => {
      csvContent += `${row.timestamp},${row.equipment_id},${row.rf_source_power},${row.rf_bias_power},${row.pressure},${row.cl2_flow},${row.hbr_flow},${row.etch_rate},${row.etch_depth},${row.oes_intensity},${row.status},${row.result}\n`;
    });

    const now = new Date();
    const pad = (n) => n.toString().padStart(2, '0');
    const dateStr = `${now.getFullYear()}${pad(now.getMonth()+1)}${pad(now.getDate())}_${pad(now.getHours())}${pad(now.getMinutes())}${pad(now.getSeconds())}`;
    const filename = `equipment_log_${dateStr}.csv`;

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.setAttribute("href", url);
    link.setAttribute("download", filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    writeLog(`CSV log data exported. Saved as "${filename}".`, 'success');
  }

  function injectManualFault() {
    if (!isRunning) {
      writeLog(`Cannot inject fault: Simulation is not running.`, 'warning');
      return;
    }

    if (!activeAnomaly) {
      const coin = Math.random() > 0.5;
      if (coin) {
        activeAnomaly = 'gas_low';
        anomalySteps = 0;
        writeLog(`MANUAL FAULT INJECTED: Gas Flow Leak! Cl2 dropping.`, 'warning');
      } else {
        activeAnomaly = 'rf_fault';
        anomalySteps = 0;
        writeLog(`MANUAL FAULT INJECTED: RF Generator Instability! Reflection high.`, 'fault');
      }
    } else {
      activeAnomaly = null;
      writeLog(`Manual anomaly cleared. Restoring chamber normal operations.`, 'info');
      telemetry.status = "NORMAL";
    }
    updateUI();
  }

  // --- Event Listeners Bindings ---
  btnStart.addEventListener('click', startSimulation);
  btnPause.addEventListener('click', pauseSimulation);
  btnStop.addEventListener('click', stopSimulation);
  btnAbort.addEventListener('click', abortSimulation);
  btnReset.addEventListener('click', resetSimulation);
  btnSaveRecipe.addEventListener('click', saveRecipe);
  btnExportLog.addEventListener('click', exportCSV);
  btnInjectFault.addEventListener('click', injectManualFault);

  toggleOverride.addEventListener('change', (e) => {
    overrideMode = e.target.checked;
    if (overrideMode) {
      elModeText.textContent = "MANUAL";
      elModeText.className = "mode-status text-warning";
      sectionManualInputs.classList.remove('disabled');
      inRfSource.disabled = false;
      inRfBias.disabled = false;
      inCl2Flow.disabled = false;
      inHbrFlow.disabled = false;
      inPressure.disabled = false;
      inTemp.disabled = false;
      writeLog(`Operator switched to MANUAL input override mode.`, 'warning');
    } else {
      elModeText.textContent = "RECIPE";
      elModeText.className = "mode-status text-cyan";
      sectionManualInputs.classList.add('disabled');
      inRfSource.disabled = true;
      inRfBias.disabled = true;
      inCl2Flow.disabled = true;
      inHbrFlow.disabled = true;
      inPressure.disabled = true;
      inTemp.disabled = true;
      writeLog(`Restored to AUTOMATIC RECIPE control sequence.`, 'info');
    }
  });

  inRfSource.addEventListener('input', (e) => {
    manualInputs.rfSource = parseInt(e.target.value);
    valRfSource.textContent = e.target.value;
  });
  
  inRfBias.addEventListener('input', (e) => {
    manualInputs.rfBias = parseInt(e.target.value);
    valRfBias.textContent = e.target.value;
  });
  
  inCl2Flow.addEventListener('input', (e) => {
    manualInputs.cl2 = parseInt(e.target.value);
    valCl2Flow.textContent = e.target.value;
  });
  
  inHbrFlow.addEventListener('input', (e) => {
    manualInputs.hbr = parseInt(e.target.value);
    valHbrFlow.textContent = e.target.value;
  });
  
  inPressure.addEventListener('input', (e) => {
    manualInputs.pressure = parseFloat(e.target.value);
  });
  
  inTemp.addEventListener('input', (e) => {
    manualInputs.temp = parseFloat(e.target.value);
  });

  resetSimulation();

});
