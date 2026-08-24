const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const scoreElement = document.getElementById('score');

const gridSize = 20;
const tileCount = canvas.width / gridSize;

const baseSpeed = 130; 
const minSpeed = 50; 

let snakeX, snakeY;
let velocityX, velocityY;

let trail = [];
let oldTrail = [];

let tail;
let foodX, foodY;
let foodType = 'normal';
let foodColor = '#ff4757';
let score;

let isGameOver = false;
let logicTimeoutId; 
let renderFrameId;
let changingDirection = false; 

let currentSpeed = baseSpeed;
let lastUpdateTime = 0;

let particles = [];
let audioCtx;

function initAudio() {
    if (!audioCtx) {
        audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    }
}

function playSound(type) {
    if (!audioCtx) return;
    if (audioCtx.state === 'suspended') audioCtx.resume();
    
    const osc = audioCtx.createOscillator();
    const gainNode = audioCtx.createGain();
    
    osc.connect(gainNode);
    gainNode.connect(audioCtx.destination);
    
    const now = audioCtx.currentTime;
    
    if (type === 'eat') {
        osc.type = 'sine';
        osc.frequency.setValueAtTime(800, now);
        osc.frequency.exponentialRampToValueAtTime(1200, now + 0.1);
        gainNode.gain.setValueAtTime(0.2, now);
        gainNode.gain.exponentialRampToValueAtTime(0.01, now + 0.1);
        osc.start(now);
        osc.stop(now + 0.1);
    } else if (type === 'golden') {
        osc.type = 'sine';
        osc.frequency.setValueAtTime(1200, now);
        osc.frequency.setValueAtTime(1600, now + 0.1);
        gainNode.gain.setValueAtTime(0.3, now);
        gainNode.gain.exponentialRampToValueAtTime(0.01, now + 0.3);
        osc.start(now);
        osc.stop(now + 0.3);
    } else if (type === 'potion') {
        osc.type = 'square'; 
        osc.frequency.setValueAtTime(300, now);
        osc.frequency.exponentialRampToValueAtTime(150, now + 0.2);
        gainNode.gain.setValueAtTime(0.2, now);
        gainNode.gain.exponentialRampToValueAtTime(0.01, now + 0.2);
        osc.start(now);
        osc.stop(now + 0.2);
    } else if (type === 'die') {
        osc.type = 'sawtooth'; 
        osc.frequency.setValueAtTime(300, now);
        osc.frequency.exponentialRampToValueAtTime(50, now + 0.5);
        gainNode.gain.setValueAtTime(0.3, now);
        gainNode.gain.exponentialRampToValueAtTime(0.01, now + 0.5);
        osc.start(now);
        osc.stop(now + 0.5);
    }
}

resetGame();

function resetGame() {
    snakeX = Math.floor(tileCount / 2);
    snakeY = Math.floor(tileCount / 2);
    velocityX = 0;
    velocityY = 0;
    tail = 1;
    score = 0;
    scoreElement.innerText = score;
    
    trail = [{x: snakeX, y: snakeY}];
    oldTrail = [{x: snakeX, y: snakeY}];
    particles = [];
    
    isGameOver = false;
    changingDirection = false;
    currentSpeed = baseSpeed;
    
    spawnFood();
    
    if (logicTimeoutId) clearTimeout(logicTimeoutId);
    if (renderFrameId) cancelAnimationFrame(renderFrameId);
    
    lastUpdateTime = performance.now();
    gameLogicLoop();
    renderLoop();
}

function gameLogicLoop() {
    if (isGameOver) return;
    
    update();
    
    if (!isGameOver) {
        let speedDecrease = Math.floor(score / 5) * 8;
        currentSpeed = Math.max(minSpeed, baseSpeed - speedDecrease);
        
        logicTimeoutId = setTimeout(gameLogicLoop, currentSpeed);
    }
}

function renderLoop() {
    draw();
    if (!isGameOver) {
        renderFrameId = requestAnimationFrame(renderLoop);
    }
}

function update() {
    if (velocityX === 0 && velocityY === 0) {
        lastUpdateTime = performance.now();
        return;
    }

    oldTrail = trail.map(segment => ({x: segment.x, y: segment.y}));

    changingDirection = false;
    lastUpdateTime = performance.now();

    snakeX += velocityX;
    snakeY += velocityY;

    if (snakeX < 0 || snakeX >= tileCount || snakeY < 0 || snakeY >= tileCount) {
        gameOver();
        return;
    }

    for (let i = 0; i < trail.length; i++) {
        if (trail[i].x === snakeX && trail[i].y === snakeY) {
            gameOver();
            return;
        }
    }

    trail.push({x: snakeX, y: snakeY});
    
    while(trail.length > tail) {
        trail.shift();
    }
    
    while(oldTrail.length < trail.length) {
        oldTrail.unshift({x: oldTrail[0].x, y: oldTrail[0].y}); 
    }

    if (snakeX === foodX && snakeY === foodY) {
        if (foodType === 'normal') {
            tail += 1;
            score += 1;
            playSound('eat');
        } else if (foodType === 'golden') {
            tail += 1;
            score += 3;
            playSound('golden');
        } else if (foodType === 'potion') {
            tail += 2;
            score += 1;
            playSound('potion');
        }
        scoreElement.innerText = score;
        
        spawnParticles(foodX * gridSize + gridSize/2, foodY * gridSize + gridSize/2, foodColor);
        spawnFood();
    }
}

function spawnFood() {
    let emptySpaces = [];
    for (let x = 0; x < tileCount; x++) {
        for (let y = 0; y < tileCount; y++) {
            let isOccupied = false;
            for (let i = 0; i < trail.length; i++) {
                if (trail[i].x === x && trail[i].y === y) {
                    isOccupied = true;
                    break;
                }
            }
            if (!isOccupied) {
                emptySpaces.push({x, y});
            }
        }
    }
    
    if (emptySpaces.length > 0) {
        let randomIndex = Math.floor(Math.random() * emptySpaces.length);
        foodX = emptySpaces[randomIndex].x;
        foodY = emptySpaces[randomIndex].y;
        
        const rand = Math.random();
        if (rand < 0.7) {
            foodType = 'normal';
            foodColor = '#ff4757';
        } else if (rand < 0.9) {
            foodType = 'golden';
            foodColor = '#f1c40f';
        } else {
            foodType = 'potion';
            foodColor = '#9b59b6';
        }
    }
}

function spawnParticles(x, y, color) {
    for(let i=0; i<15; i++) {
        const angle = Math.random() * Math.PI * 2;
        const speed = Math.random() * 3 + 1;
        particles.push({
            x: x,
            y: y,
            vx: Math.cos(angle) * speed,
            vy: Math.sin(angle) * speed,
            life: 20 + Math.random() * 15,
            maxLife: 35,
            color: color,
            size: Math.random() * 3 + 1
        });
    }
}

function lerp(start, end, t) {
    return start + (end - start) * t;
}

function draw() {
    let now = performance.now();
    let t = (velocityX === 0 && velocityY === 0) ? 0 : (now - lastUpdateTime) / currentSpeed;
    if (t > 1) t = 1;
    if (isGameOver) t = 1; 

    ctx.fillStyle = '#121212';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.strokeStyle = 'rgba(255, 255, 255, 0.03)'; 
    ctx.lineWidth = 1;
    for(let i = 0; i < tileCount; i++) {
        ctx.beginPath();
        ctx.moveTo(i * gridSize, 0);
        ctx.lineTo(i * gridSize, canvas.height);
        ctx.stroke();
        
        ctx.beginPath();
        ctx.moveTo(0, i * gridSize);
        ctx.lineTo(canvas.width, i * gridSize);
        ctx.stroke();
    }

    const fCenterX = foodX * gridSize + gridSize / 2;
    const fCenterY = foodY * gridSize + gridSize / 2;
    const pulse = 1 + Math.sin(now / 150) * 0.1; 

    if (foodType === 'normal' || foodType === 'golden') {
        if (foodType === 'golden') {
            ctx.shadowBlur = 15;
            ctx.shadowColor = foodColor;
        }
        
        ctx.beginPath();
        ctx.arc(fCenterX, fCenterY, (gridSize / 2 - 2) * pulse, 0, Math.PI * 2);
        ctx.fillStyle = foodColor; 
        ctx.fill();
        ctx.shadowBlur = 0; 
        
        ctx.beginPath();
        ctx.arc(fCenterX - 3 * pulse, fCenterY - 3 * pulse, 3 * pulse, 0, Math.PI * 2);
        ctx.fillStyle = 'rgba(255, 255, 255, 0.6)';
        ctx.fill();
    } else if (foodType === 'potion') {
        ctx.fillStyle = foodColor;
        ctx.beginPath();
        ctx.moveTo(fCenterX, fCenterY - 6 * pulse);
        ctx.lineTo(fCenterX - 7 * pulse, fCenterY + 6 * pulse);
        ctx.lineTo(fCenterX + 7 * pulse, fCenterY + 6 * pulse);
        ctx.closePath();
        ctx.fill();
        
        ctx.fillRect(fCenterX - 3 * pulse, fCenterY - 8 * pulse, 6 * pulse, 4 * pulse);
        
        ctx.fillStyle = 'rgba(255, 255, 255, 0.5)';
        ctx.beginPath();
        ctx.arc(fCenterX, fCenterY + 3 * pulse, 2 * pulse, 0, Math.PI * 2);
        ctx.fill();
    }

    for (let i = particles.length - 1; i >= 0; i--) {
        let p = particles[i];
        p.x += p.vx;
        p.y += p.vy;
        p.life--;
        if (p.life <= 0) {
            particles.splice(i, 1);
            continue;
        }
        ctx.globalAlpha = p.life / p.maxLife;
        ctx.fillStyle = p.color;
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
        ctx.fill();
    }
    ctx.globalAlpha = 1.0; 

    let pathPoints = [];
    if (trail.length > 0 && oldTrail.length === trail.length) {
        pathPoints.push({
            x: lerp(oldTrail[0].x, trail[0].x, t),
            y: lerp(oldTrail[0].y, trail[0].y, t)
        });

        for (let i = 0; i < trail.length - 1; i++) {
            pathPoints.push(trail[i]);
        }

        let lastIdx = trail.length - 1;
        pathPoints.push({
            x: lerp(oldTrail[lastIdx].x, trail[lastIdx].x, t),
            y: lerp(oldTrail[lastIdx].y, trail[lastIdx].y, t)
        });

        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        ctx.lineWidth = gridSize - 2;

        for (let i = 0; i < pathPoints.length - 1; i++) {
            const p1 = pathPoints[i];
            const p2 = pathPoints[i+1];
            
            if (p1.x === p2.x && p1.y === p2.y) continue;
            
            const lightness = 15 + (i / (pathPoints.length - 1)) * 45; 
            ctx.strokeStyle = `hsl(145, 70%, ${lightness}%)`;
            
            ctx.beginPath();
            ctx.moveTo(p1.x * gridSize + gridSize / 2, p1.y * gridSize + gridSize / 2);
            ctx.lineTo(p2.x * gridSize + gridSize / 2, p2.y * gridSize + gridSize / 2);
            ctx.stroke();
        }

        const head = pathPoints[pathPoints.length - 1];
        ctx.fillStyle = '#7bed9f';
        ctx.beginPath();
        ctx.arc(head.x * gridSize + gridSize / 2, head.y * gridSize + gridSize / 2, (gridSize - 2) / 2, 0, Math.PI * 2);
        ctx.fill();
        
        let dirX = velocityX;
        let dirY = velocityY;
        if (dirX === 0 && dirY === 0) { dirX = 1; dirY = 0; } 
        
        const angle = Math.atan2(dirY, dirX);
        const eyeOffsetDist = 4; 
        const eyeAngleOffset = Math.PI / 4; 

        const hX = head.x * gridSize + gridSize/2;
        const hY = head.y * gridSize + gridSize/2;

        const eye1X = hX + Math.cos(angle - eyeAngleOffset) * eyeOffsetDist;
        const eye1Y = hY + Math.sin(angle - eyeAngleOffset) * eyeOffsetDist;

        const eye2X = hX + Math.cos(angle + eyeAngleOffset) * eyeOffsetDist;
        const eye2Y = hY + Math.sin(angle + eyeAngleOffset) * eyeOffsetDist;

        ctx.fillStyle = 'white';
        ctx.beginPath(); ctx.arc(eye1X, eye1Y, 3.5, 0, Math.PI*2); ctx.fill();
        ctx.beginPath(); ctx.arc(eye2X, eye2Y, 3.5, 0, Math.PI*2); ctx.fill();

        ctx.fillStyle = 'black';
        const pupilDist = 1.5;
        ctx.beginPath(); ctx.arc(eye1X + dirX*pupilDist, eye1Y + dirY*pupilDist, 1.8, 0, Math.PI*2); ctx.fill();
        ctx.beginPath(); ctx.arc(eye2X + dirX*pupilDist, eye2Y + dirY*pupilDist, 1.8, 0, Math.PI*2); ctx.fill();
    }

    if (isGameOver) {
        ctx.fillStyle = 'rgba(0, 0, 0, 0.85)'; 
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        
        ctx.fillStyle = '#ff4757';
        ctx.font = '800 48px "Outfit", sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('GAME OVER', canvas.width / 2, canvas.height / 2 - 30);
        
        ctx.fillStyle = 'white';
        ctx.font = '600 24px "Outfit", sans-serif';
        ctx.fillText('FINAL SCORE: ' + score, canvas.width / 2, canvas.height / 2 + 20);
        
        ctx.fillStyle = 'rgba(255,255,255,0.5)';
        ctx.font = '400 16px "Outfit", sans-serif';
        // 모바일과 PC 모두 아우르는 문구로 변경
        ctx.fillText("Press 'Space' or Tap Canvas to Restart", canvas.width / 2, canvas.height / 2 + 65);
    }
}

function gameOver() {
    if (!isGameOver) playSound('die'); 
    isGameOver = true;
    if (logicTimeoutId) clearTimeout(logicTimeoutId);
}

// ---------------- 방향 조작 로직 통합 ----------------
function handleDirection(dir) {
    initAudio(); 

    if (isGameOver) return; 
    if (changingDirection) return;

    switch(dir) {
        case 'up':
            if (velocityY !== 1) { 
                velocityX = 0;
                velocityY = -1;
                changingDirection = true;
            }
            break;
        case 'down':
            if (velocityY !== -1) {
                velocityX = 0;
                velocityY = 1;
                changingDirection = true;
            }
            break;
        case 'left':
            if (velocityX !== 1) {
                velocityX = -1;
                velocityY = 0;
                changingDirection = true;
            }
            break;
        case 'right':
            if (velocityX !== -1) {
                velocityX = 1;
                velocityY = 0;
                changingDirection = true;
            }
            break;
    }
}

// 키보드 이벤트 청취 (데스크탑)
document.addEventListener('keydown', (e) => {
    initAudio(); 

    if (isGameOver && e.code === 'Space') {
        e.preventDefault();
        resetGame();
        return;
    }

    if (isGameOver) return; 

    // 방향키 매핑
    switch(e.key) {
        case 'ArrowUp': case 'w': case 'W':
            e.preventDefault();
            handleDirection('up');
            break;
        case 'ArrowDown': case 's': case 'S':
            e.preventDefault();
            handleDirection('down');
            break;
        case 'ArrowLeft': case 'a': case 'A':
            e.preventDefault();
            handleDirection('left');
            break;
        case 'ArrowRight': case 'd': case 'D':
            e.preventDefault();
            handleDirection('right');
            break;
    }
});

// 가상 방향키 패드 터치 이벤트 (모바일)
// click과 touchstart를 모두 커버할 수 있는 pointerdown 사용
['up', 'down', 'left', 'right'].forEach(dir => {
    const btn = document.getElementById('btn-' + dir);
    if (btn) {
        btn.addEventListener('pointerdown', (e) => {
            e.preventDefault(); // 스크롤 방지, 더블탭 확대 방지
            handleDirection(dir);
        });
    }
});

// 캔버스 터치 재시작 로직 (모바일)
canvas.addEventListener('pointerdown', (e) => {
    initAudio();
    if (isGameOver) {
        e.preventDefault();
        resetGame();
    }
});
