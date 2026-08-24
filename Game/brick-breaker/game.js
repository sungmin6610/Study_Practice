/*
 * game.js
 * 벽돌깨기 게임의 실질적인 동작(움직임, 점수 계산, 물리 충돌, 효과음 등)을 제어하는 스크립트입니다.
 */

// 1. HTML의 Canvas 요소를 자바스크립트로 연결하여 조작할 도구를 마련합니다.
const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d'); // 2D 그래픽을 그리기 위한 '붓' 역할을 하는 컨텍스트입니다.

// 2. 패들(Paddle - 플레이어가 조작하는 아래 바)의 설정 값들입니다.
let basePaddleWidth = 100; // 패들의 기본 가로 크기(픽셀)
let paddleWidth = basePaddleWidth; // 현재 패들의 가로 크기 (아이템 획득 시 2배인 200px로 동적 변경)
const paddleHeight = 12; // 패들의 세로 크기(픽셀)
const paddleY = 560; // 패들의 세로축 위치 (화면 상단이 y=0이고, 아래로 갈수록 커짐)
let paddleX = (canvas.width - paddleWidth) / 2; // 패들의 가로축 위치 (화면 정중앙으로 초기화)
const paddleSpeed = 7; // 한 프레임(약 1/60초)당 패들이 움직일 거리(속도)

// 2-1. 파워업 아이템(Power-up Item) 설정 값들입니다.
let items = []; // 떨어지고 있는 파워업 아이템들을 담을 배열
const itemWidth = 16; // 아이템의 가로 크기(픽셀)
const itemHeight = 16; // 아이템의 세로 크기(픽셀)
const itemSpeed = 2; // 아이템이 밑으로 떨어지는 속도(픽셀/프레임)
const powerUpDuration = 10000; // 파워업 지속 시간 (10초 = 10000밀리초)
let powerUpTimer = 0; // 파워업이 종료될 타임스탬프 시각
let isPowerUpActive = false; // 현재 파워업(2배 길어짐) 상태가 켜져있는지 여부
let isLaserActive = false; // 레이저 상태 활성화 여부
let laserTimer = 0; // 레이저 지속 시간 만료 시각

// 2-2. 레이저 미사일 설정 값들입니다.
let missiles = [];
const missileWidth = 4;
const missileHeight = 15;
const missileSpeed = 8;

// 3. 공(Ball)의 설정 값들입니다.
const ballRadius = 8; // 공의 반지름(픽셀)
let baseSpeed = Math.sqrt(4 * 4 + (-4) * (-4)); // 피타고라스 정리로 공의 실제 전체 속력 구하기 (~5.66 픽셀/프레임)
let balls = []; // 다중 공 관리를 위한 배열

// 3-1. 파티클(파편) 배열 및 생성 함수
let particles = [];
function spawnParticles(x, y, color) {
    const numParticles = Math.floor(Math.random() * 6) + 10; // 10 ~ 15개
    for (let i = 0; i < numParticles; i++) {
        particles.push({
            x: x,
            y: y,
            dx: (Math.random() - 0.5) * 8,
            dy: (Math.random() - 0.5) * 8,
            radius: Math.random() * 3 + 1,
            color: color,
            life: 1.0,
            decay: Math.random() * 0.03 + 0.015
        });
    }
}

// 4. 벽돌(Bricks)의 구조 및 크기 설정 값들입니다.
let brickRows = 5; // 벽돌의 줄(행) 개수
const brickCols = 10; // 벽돌의 칸(열) 개수
const brickWidth = 68; // 벽돌 1개의 가로 크기(픽셀)
const brickHeight = 22; // 벽돌 1개의 세로 크기(픽셀)
const brickPaddingX = 4; // 벽돌 사이의 가로 여백(간격)
const brickPaddingY = 4; // 벽돌 사이의 세로 여백(간격)
const brickOffsetLeft = 60; // 전체 벽돌 영역이 왼쪽 벽에서 떨어질 시작점 간격
const brickOffsetTop = 60; // 전체 벽돌 영역이 천장에서 떨어질 시작점 간격

// 벽돌의 내구도별 색상 테이블입니다.
const durabilityColors = {
    1: '#2E9E5B', // 내구도 1: 초록색
    2: '#E8A33D', // 내구도 2: 주황색
    3: '#E05A4E'  // 내구도 3: 빨간색
};

let bricks = []; // 게임에 등장할 모든 벽돌의 좌표와 생사 상태를 보관할 2차원 배열입니다.

// 모든 벽돌의 초기 위치와 속성을 정해주는 함수입니다.
function initBricks() {
    bricks = [];
    for (let r = 0; r < brickRows; r++) {
        bricks[r] = [];
        for (let c = 0; c < brickCols; c++) {
            // 수학 공식: 벽돌의 칸/줄 번호에 크기와 여백을 곱하여 정확한 화면 배치 좌표를 구합니다.
            const brickX = c * (brickWidth + brickPaddingX) + brickOffsetLeft;
            const brickY = r * (brickHeight + brickPaddingY) + brickOffsetTop;
            
            let durability = 1;
            const rand = Math.random();
            
            // 스테이지별 내구도 확률 차등 적용
            if (currentStage === 1) {
                if (rand < 0.70) durability = 1;      // 70%
                else if (rand < 0.90) durability = 2; // 20%
                else durability = 3;                  // 10%
            } else if (currentStage === 2) {
                if (rand < 0.50) durability = 1;      // 50%
                else if (rand < 0.80) durability = 2; // 30%
                else durability = 3;                  // 20%
            } else {
                // 스테이지 3 이상
                if (rand < 0.40) durability = 1;      // 40%
                else if (rand < 0.70) durability = 2; // 30%
                else durability = 3;                  // 30%
            }
            
            bricks[r][c] = {
                x: brickX,
                y: brickY,
                status: durability
            };
        }
    }
}

// 아이템 스폰 함수 (현재 총 16% 확률)
function spawnItem(x, y) {
    if (Math.random() < 0.16) {
        const rNum = Math.random();
        let type = 'expand';
        if (rNum < 0.33) {
            type = 'multiball';
        } else if (rNum < 0.66) {
            type = 'laser';
        }
        items.push({ x: x, y: y, type: type });
    }
}

// 5. 웹 오디오 API (Web Audio API)를 이용한 사운드 효과 설정입니다.
// 별도 mp3 파일 다운로드 없이 브라우저 자체 주파수 합성 기술을 이용해 효과음을 만듭니다.
let audioCtx = null;

// 효과음을 사용하기 전 오디오 시스템을 최초 한 번 초기화하는 함수입니다.
function initAudio() {
    if (!audioCtx) {
        try {
            // 브라우저마다 이름이 다를 수 있어 크로스 브라우징을 처리해줍니다.
            audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        } catch (e) {
            console.warn("오디오 시스템을 활성화할 수 없습니다.", e);
        }
    }
}

// 특정 주파수(freq)와 재생 시간(duration)을 주면 '삐-'하는 기계 비프음을 생성하는 만능 함수입니다.
function playBeep(freq, duration) {
    initAudio();
    if (!audioCtx) return;
    
    // 브라우저의 보안 정책상, 사용자가 상호작용하기 전엔 오디오가 정지(suspended)될 수 있어 활성화해줍니다.
    if (audioCtx.state === 'suspended') {
        audioCtx.resume();
    }

    try {
        // 소리를 생성하는 진동자(Oscillator) 노드와 음량을 조절하는 증폭기(Gain) 노드를 생성합니다.
        const osc = audioCtx.createOscillator();
        const gainNode = audioCtx.createGain();

        // 노드 연결: 진동자 -> 증폭기 -> 최종 컴퓨터 스피커(destination)
        osc.connect(gainNode);
        gainNode.connect(audioCtx.destination);

        osc.type = 'sine'; // 파형의 형태는 가장 맑고 깨끗한 사인파로 지정합니다.
        osc.frequency.setValueAtTime(freq, audioCtx.currentTime); // 주파수 입력 (헤르츠 단위)

        // 음량 조절: 재생 직후엔 음량 0.08로 출력하다가 소리 끝날 때쯤 서서히 음량 0에 수렴하게 만듭니다.
        // 이 볼륨 조절(볼륨 포장지 - Envelope)을 통해 효과음의 끝이 뚝 끊겨 찢어지는 잡음을 방지합니다.
        gainNode.gain.setValueAtTime(0.08, audioCtx.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + duration);

        osc.start(audioCtx.currentTime); // 진동자 시작
        osc.stop(audioCtx.currentTime + duration); // 진동자 예약 종료
    } catch (e) {
        console.warn("효과음 재생 도중 에러 발생:", e);
    }
}

// 게임 오버 BGM (슬픈 하강 멜로디)
function playGameOverBGM() {
    playBeep(400, 0.3);
    setTimeout(() => playBeep(300, 0.3), 300);
    setTimeout(() => playBeep(200, 0.4), 600);
    setTimeout(() => playBeep(150, 0.6), 1000);
}

// 스테이지 클리어 BGM (경쾌한 상승 멜로디)
function playStageClearBGM() {
    playBeep(523.25, 0.15); // C5
    setTimeout(() => playBeep(659.25, 0.15), 150); // E5
    setTimeout(() => playBeep(783.99, 0.15), 300); // G5
    setTimeout(() => playBeep(1046.50, 0.4), 450); // C6
}

// 레이저 발사 효과음 (총소리/화이트 노이즈 기반)
function playGunshot() {
    initAudio();
    if (!audioCtx) return;
    
    if (audioCtx.state === 'suspended') {
        audioCtx.resume();
    }

    try {
        const duration = 0.15;
        const bufferSize = audioCtx.sampleRate * duration;
        const buffer = audioCtx.createBuffer(1, bufferSize, audioCtx.sampleRate);
        const data = buffer.getChannelData(0);
        
        // 화이트 노이즈 생성
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1; 
        }
        
        const noiseSource = audioCtx.createBufferSource();
        noiseSource.buffer = buffer;
        
        // 저주파 통과 필터로 총소리 특유의 둔탁하고 강한 느낌(펀치감) 주기
        const filter = audioCtx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(1000, audioCtx.currentTime);
        filter.frequency.exponentialRampToValueAtTime(100, audioCtx.currentTime + duration);
        
        const gainNode = audioCtx.createGain();
        gainNode.gain.setValueAtTime(0.4, audioCtx.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioCtx.currentTime + duration);
        
        noiseSource.connect(filter);
        filter.connect(gainNode);
        gainNode.connect(audioCtx.destination);
        
        noiseSource.start(audioCtx.currentTime);
    } catch (e) {
        console.warn("효과음 재생 도중 에러 발생:", e);
    }
}

// 6. 게임의 핵심적인 논리 상태를 기록하는 변수들입니다.
let score = 0; // 현재 득점한 누적 점수
let highScore = parseInt(localStorage.getItem('brickBreakerHighScore')) || 0; // 브라우저 로컬 스토리지에 저장된 최고 점수
let lives = 3; // 남은 플레이어의 생명 개수 (하트로 생각하면 됩니다)
let currentStage = 1; // 현재 스테이지 단계
let isGameLaunched = false; // 아예 게임을 처음 켜서 시작 안내 창이 열려있는 대기 화면 상태인지 여부
let isGameActive = false;   // 게임 시뮬레이션(공의 움직임 등)이 실제로 돌고 있는지 여부
let isGameWon = false;      // 모든 벽돌을 다 깨서 승리했는지 여부
let isWaitingToStart = false; // 공이 아래로 빠지고 다음 공을 주기 전 1초간 카운트다운를 기다리는 상태인지 여부
let waitTimerStart = 0;      // 1초 정지를 시작한 시각의 타임스탬프 저장 변수

initBricks(); // 제반 변수(currentStage 등)가 모두 초기화된 후 벽돌을 최초로 1회 생성합니다.

// 7. 키보드 눌림 입력을 실시간으로 체크하기 위한 상태 플래그(깃발)들입니다.
let rightPressed = false; // 오른쪽 화살표 키가 현재 눌려있는가?
let leftPressed = false; // 왼쪽 화살표 키가 현재 눌려있는가?

function handleAction() {
    // 스페이스바와 동일한 동작을 수행합니다.
    if (!isGameLaunched) {
        // 시작 대기 화면이었다면, 게임 정식 론칭!
        isGameLaunched = true;
        isGameActive = true;
        paddleX = (canvas.width - paddleWidth) / 2;
        balls = [{
            x: paddleX + paddleWidth / 2,
            y: paddleY - ballRadius,
            dx: baseSpeed * Math.sin(Math.PI / 4),
            dy: -baseSpeed * Math.cos(Math.PI / 4)
        }];
    } else if (!isGameActive) {
        // 게임 오버/승리 일시정지 시 새 게임 시작
        resetGame();
    } else if (isLaserActive && !isWaitingToStart) {
        // 레이저 발사
        missiles.push({ x: paddleX + 4, y: paddleY - 6 });
        missiles.push({ x: paddleX + paddleWidth - 10, y: paddleY - 6 });
        playGunshot(); // 총소리 효과음
    }
}

// Add keyboard event listeners
document.addEventListener('keydown', keyDownHandler, false);
document.addEventListener('keyup', keyUpHandler, false);

// Touch event listeners for mobile devices
canvas.addEventListener('touchstart', touchHandler, { passive: false });
canvas.addEventListener('touchmove', touchHandler, { passive: false });

// 마우스로 브라우저 화면을 아무 곳이나 클릭해도 오디오 기능이 안전하게 활성화되게 돕는 백업 코드입니다.
document.addEventListener('click', () => {
    initAudio();
});

// 키보드 키가 꾹 눌릴 때 실행되는 함수입니다.
function keyDownHandler(e) {
    initAudio(); // 최초 키 입력 시 사운드 시스템 활성화
    if (e.key === 'Right' || e.key === 'ArrowRight') {
        rightPressed = true;
    } else if (e.key === 'Left' || e.key === 'ArrowLeft') {
        leftPressed = true;
    } else if (e.key === ' ' || e.code === 'Space') {
        handleAction();
    }
}

// 키보드에서 손을 떼는 순간 실행되는 함수입니다. 움직임을 멈춥니다.
function keyUpHandler(e) {
    if (e.key === 'Right' || e.key === 'ArrowRight') {
        rightPressed = false;
    } else if (e.key === 'Left' || e.key === 'ArrowLeft') {
        leftPressed = false;
    }
}

function touchHandler(e) {
    e.preventDefault(); // 기본 스크롤 방지
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width; // 실제 캔버스 픽셀 대비 화면 비율
    const touch = e.touches[0];
    const x = (touch.clientX - rect.left) * scaleX;
    // 패들 중앙을 터치 위치에 맞춤
    paddleX = x - paddleWidth / 2;
    // 화면 밖으로 나가지 않게 제한
    if (paddleX < 0) paddleX = 0;
    if (paddleX + paddleWidth > canvas.width) paddleX = canvas.width - paddleWidth;
    if (e.type === 'touchstart') {
        handleAction(); // 스페이스바와 동일 동작
    }
}

// 죽었거나, 다 깨서 완전히 게임판을 태초의 상태로 초기화하여 재시작할 때 작동하는 함수입니다.
function resetGame() {
    currentStage = 1;
    brickRows = 5; // 기본 5줄
    baseSpeed = Math.sqrt(4 * 4 + (-4) * (-4)); // 기본 속도
    basePaddleWidth = 100; // 패들 기본 크기 초기화
    paddleWidth = basePaddleWidth; // 패들 크기 복구
    paddleX = (canvas.width - paddleWidth) / 2;
    balls = [{
        x: paddleX + paddleWidth / 2,
        y: paddleY - ballRadius,
        dx: baseSpeed * Math.sin(Math.PI / 4),
        dy: -baseSpeed * Math.cos(Math.PI / 4)
    }];
    score = 0;
    lives = 3;
    initBricks(); // 벽돌 다시 살리기
    items = []; // 떨어지던 아이템들 싹 지우기
    missiles = []; // 미사일 싹 지우기
    particles = []; // 파티클 싹 지우기
    isPowerUpActive = false; // 파워업 비활성화
    isLaserActive = false; // 레이저 비활성화
    powerUpTimer = 0;
    laserTimer = 0;
    isGameActive = true;
    isGameWon = false;
    isWaitingToStart = false;
    isGameLaunched = true;
}

// 모든 벽돌을 파괴했을 때 다음 단계로 진입하는 함수입니다.
function nextStage() {
    currentStage++;
    brickRows = Math.min(7, brickRows + 1); // 행 1개씩 추가 (최대 7줄)
    baseSpeed *= 1.2; // 공 속도 20% 증가
    
    // 스테이지 3 이상 진입 시 패들 크기 0.8배 축소 (최소 40픽셀)
    if (currentStage >= 3) {
        basePaddleWidth = Math.max(40, basePaddleWidth * 0.8);
    }
    
    paddleWidth = basePaddleWidth;
    paddleX = (canvas.width - paddleWidth) / 2;
    balls = [{
        x: paddleX + paddleWidth / 2,
        y: paddleY - ballRadius,
        dx: baseSpeed * Math.sin(Math.PI / 4),
        dy: -baseSpeed * Math.cos(Math.PI / 4)
    }];
    
    initBricks();
    items = [];
    missiles = [];
    particles = [];
    isPowerUpActive = false;
    isLaserActive = false;
    powerUpTimer = 0;
    laserTimer = 0;
    
    playStageClearBGM(); // 스테이지 클리어 효과음 재생
    
    isWaitingToStart = true;
    waitTimerStart = Date.now();
}

// 8. 물리 충돌 및 갱신 논리 로직입니다.
// 현재 살아있는 모든 벽돌에 대한 승리 조건 검증
function checkWinCondition() {
    for (let r = 0; r < brickRows; r++) {
        for (let c = 0; c < brickCols; c++) {
            if (bricks[r][c].status > 0) {
                return false; // 아직 부서지지 않은 벽돌이 1개라도 있다면 승리가 아닙니다.
            }
        }
    }
    return true; // 루프를 다 도는 동안 남은 벽돌이 없다면 승리!
}

// 공과 벽돌의 2차원 바운딩 박스(AABB) 충돌을 정밀 감지하고 반사 방향을 구합니다.
function collisionDetection() {
    for (let r = 0; r < brickRows; r++) {
        for (let c = 0; c < brickCols; c++) {
            const b = bricks[r][c];
            if (b.status > 0) { // 해당 벽돌이 아직 깨지지 않고 살아있는 경우만 충돌 체크
                for (let i = 0; i < balls.length; i++) {
                    const ball = balls[i];
                    
                    // 공의 사방 끝점이 벽돌의 사각형 영역 범위와 겹치는지 체크하는 조건문입니다.
                    if (ball.x + ballRadius >= b.x && ball.x - ballRadius <= b.x + brickWidth &&
                        ball.y + ballRadius >= b.y && ball.y - ballRadius <= b.y + brickHeight) {
                        
                        b.status -= 1; // 내구도 1 감소
                        
                        if (b.status === 0) {
                            score += 10; // 완전히 파괴 시 점수 10점 획득
                            
                            // 최고 점수 갱신 및 로컬 스토리지에 저장
                            if (score > highScore) {
                                highScore = score;
                                localStorage.setItem('brickBreakerHighScore', highScore);
                            }
                            
                            playBeep(800, 0.08); // 800Hz의 맑은 폭발음 재생

                            // 파편 이펙트 생성 (내구도 1인 초록색 벽돌이 깨지는 것이므로 색상 지정)
                            spawnParticles(b.x + brickWidth / 2, b.y + brickHeight / 2, durabilityColors[1]);

                            // 파괴 시 지정된 확률로 아이템 떨어뜨리기
                            spawnItem(b.x + brickWidth / 2 - itemWidth / 2, b.y + brickHeight);

                            // 모든 벽돌 파괴 시 다음 스테이지로 즉시 진행합니다.
                            if (checkWinCondition()) {
                                nextStage();
                                return; // 더 이상 충돌 체크 안함
                            }
                        } else {
                            // 내구도가 깎였을 때 둔탁한 타격음
                            playBeep(600, 0.05);
                        }

                        // [벽돌의 상/하/좌/우 중 어디에 정밀하게 박았는지 판정하기]
                        const prevX = ball.x - ball.dx;
                        const prevY = ball.y - ball.dy;

                        if (prevY + ballRadius <= b.y) {
                            ball.y = b.y - ballRadius; 
                            ball.dy = -ball.dy; 
                        } else if (prevY - ballRadius >= b.y + brickHeight) {
                            ball.y = b.y + brickHeight + ballRadius;
                            ball.dy = -ball.dy; 
                        } else if (prevX + ballRadius <= b.x) {
                            ball.x = b.x - ballRadius;
                            ball.dx = -ball.dx; 
                        } else if (prevX - ballRadius >= b.x + brickWidth) {
                            ball.x = b.x + brickWidth + ballRadius;
                            ball.dx = -ball.dx; 
                        } else {
                            ball.dy = -ball.dy;
                        }
                        break; // 한 벽돌당 하나의 공만 처리하고 다음 벽돌로 넘어가도록 최적화
                    }
                }
            }
        }
    }
}

// 미사일 업데이트 및 충돌 검사
function updateMissiles() {
    if (!isGameActive || isWaitingToStart) return;
    
    for (let i = missiles.length - 1; i >= 0; i--) {
        const m = missiles[i];
        m.y -= missileSpeed; // 위로 상승
        
        // 화면 위로 벗어나면 소멸
        if (m.y + missileHeight < 0) {
            missiles.splice(i, 1);
            continue;
        }
        
        // 미사일과 벽돌 충돌 검사
        let hit = false;
        for (let r = 0; r < brickRows; r++) {
            for (let c = 0; c < brickCols; c++) {
                const b = bricks[r][c];
                if (b.status > 0) {
                    if (m.x + missileWidth >= b.x && m.x <= b.x + brickWidth &&
                        m.y + missileHeight >= b.y && m.y <= b.y + brickHeight) {
                        
                        b.status -= 1; // 내구도 감소
                        hit = true;
                        
                        if (b.status === 0) {
                            score += 10;
                            if (score > highScore) {
                                highScore = score;
                                localStorage.setItem('brickBreakerHighScore', highScore);
                            }
                            playBeep(800, 0.08); // 폭발음
                            
                            spawnParticles(b.x + brickWidth / 2, b.y + brickHeight / 2, durabilityColors[1]);
                            
                            // 미사일로 파괴해도 아이템 스폰 가능
                            spawnItem(b.x + brickWidth / 2 - itemWidth / 2, b.y + brickHeight);
                            
                            if (checkWinCondition()) {
                                nextStage();
                            }
                        } else {
                            playBeep(600, 0.05); // 타격음
                        }
                        break;
                    }
                }
            }
            if (hit) break;
        }
        
        if (hit) {
            missiles.splice(i, 1);
        }
    }
    
    // 레이저 모드 시간 만료 처리
    if (isLaserActive && Date.now() >= laserTimer) {
        isLaserActive = false;
        playBeep(660, 0.08);
        setTimeout(() => playBeep(440, 0.1), 60);
    }
}

// 8-1. 아이템 관련 유틸리티 물리 갱신 함수입니다.
function updateItems() {
    if (!isGameActive || isWaitingToStart) return;

    for (let i = items.length - 1; i >= 0; i--) {
        const item = items[i];
        item.y += itemSpeed; // 중력에 의해 떨어지는 모션

        // AABB 충돌 감지: 아이템과 패들의 접촉 여부 체크
        const withinPaddleX = item.x + itemWidth >= paddleX && item.x <= paddleX + paddleWidth;
        const hitPaddleY = item.y + itemHeight >= paddleY && item.y <= paddleY + paddleHeight;

        if (withinPaddleX && hitPaddleY) {
            if (item.type === 'expand') {
                playBeep(440, 0.08);
                setTimeout(() => playBeep(660, 0.1), 60);
                isPowerUpActive = true;
                powerUpTimer = Date.now() + powerUpDuration; 
                paddleWidth = basePaddleWidth * 2; 

                // 패들이 화면 밖으로 나가지 않게 보정
                if (paddleX + paddleWidth > canvas.width) {
                    paddleX = canvas.width - paddleWidth;
                }
            } else if (item.type === 'laser') {
                // 레이저 장착음
                playBeep(660, 0.08);
                setTimeout(() => playBeep(880, 0.1), 60);
                isLaserActive = true;
                laserTimer = Date.now() + 5000; // 5초간 활성화
            } else if (item.type === 'multiball') {
                // 멀티볼 분열음
                playBeep(330, 0.08);
                setTimeout(() => playBeep(550, 0.1), 60);
                
                // 기존 공 각각에 대해 좌우 각도로 새로운 공 복제 (1개 -> 3개)
                let newBalls = [];
                for (let j = 0; j < balls.length; j++) {
                    const originalBall = balls[j];
                    const currentAngle = Math.atan2(originalBall.dy, originalBall.dx);
                    
                    // -20도 방향 (라디안 기준 약 -0.35)
                    newBalls.push({
                        x: originalBall.x,
                        y: originalBall.y,
                        dx: baseSpeed * Math.cos(currentAngle - 0.35),
                        dy: baseSpeed * Math.sin(currentAngle - 0.35)
                    });
                    
                    // +20도 방향 (라디안 기준 약 +0.35)
                    newBalls.push({
                        x: originalBall.x,
                        y: originalBall.y,
                        dx: baseSpeed * Math.cos(currentAngle + 0.35),
                        dy: baseSpeed * Math.sin(currentAngle + 0.35)
                    });
                }
                // 기존 배열에 복제된 공들 합치기
                balls = balls.concat(newBalls);
            }

            items.splice(i, 1); // 획득 완료된 아이템 소멸
            continue;
        }

        // 낭떠러지로 낙하한 아이템 소멸
        if (item.y > canvas.height) {
            items.splice(i, 1);
        }
    }

    // 파워업 시간 만료 시 패들 원복 처리 (expand)
    if (isPowerUpActive) {
        if (Date.now() >= powerUpTimer) {
            playBeep(660, 0.08);
            setTimeout(() => playBeep(440, 0.1), 60);

            isPowerUpActive = false;
            paddleWidth = basePaddleWidth; 
            
            if (paddleX + paddleWidth > canvas.width) {
                paddleX = canvas.width - paddleWidth;
            }
        }
    }
}

// 매 프레임마다 모든 움직이는 물체의 좌표와 규칙을 계산하는 핵심 물리 업데이트 함수입니다.
function update() {
    // 1) 게임 시작화면이거나 끝난 정지화면이면 좌표 계산을 하지 않고 그대로 동결시킵니다.
    if (!isGameLaunched || !isGameActive) return;

    // 2) 공을 하단으로 빠뜨린 뒤 다음 생명을 주기 위한 1초 리디렉션 정지 타이머 체크
    if (isWaitingToStart) {
        if (Date.now() - waitTimerStart >= 1000) {
            isWaitingToStart = false; // 1000ms(1초)가 경과하면 자동 정지 해제되어 출격!
        } else {
            // 대기 시간 동안은 패들을 묶어두고, 남은 첫 공을 패들 중앙에 위치시킴
            paddleX = (canvas.width - paddleWidth) / 2;
            if (balls.length > 0) {
                balls[0].x = paddleX + paddleWidth / 2;
                balls[0].y = paddleY - ballRadius;
            }
            return;
        }
    }

    // 3) 키보드 작동에 맞춰 패들을 이동시킵니다.
    if (rightPressed) {
        paddleX += paddleSpeed;
        if (paddleX + paddleWidth > canvas.width) {
            paddleX = canvas.width - paddleWidth;
        }
    } else if (leftPressed) {
        paddleX -= paddleSpeed;
        if (paddleX < 0) {
            paddleX = 0;
        }
    }

    // 4) 다중 공 배열을 순회하며 움직임 및 벽/패들 충돌 연산 적용
    for (let i = balls.length - 1; i >= 0; i--) {
        const ball = balls[i];
        
        ball.x += ball.dx;
        ball.y += ball.dy;

        // 벽 반사 로직
        if (ball.x - ballRadius <= 0) {
            ball.x = ballRadius;
            ball.dx = -ball.dx;
            playBeep(320, 0.06); 
        } else if (ball.x + ballRadius >= canvas.width) {
            ball.x = canvas.width - ballRadius;
            ball.dx = -ball.dx;
            playBeep(320, 0.06);
        }

        if (ball.y - ballRadius <= 0) {
            ball.y = ballRadius;
            ball.dy = -ball.dy;
            playBeep(320, 0.06);
        }

        // 바닥으로 떨어진 공 제거
        if (ball.y + ballRadius > canvas.height) {
            balls.splice(i, 1);
            continue; // 제거되었으므로 패들 반사 로직은 스킵
        }

        // 공과 패들의 반사 물리 연산 (공이 아래로 하강 중일 때만)
        if (ball.dy > 0) {
            const withinPaddleX = ball.x + ballRadius >= paddleX && ball.x - ballRadius <= paddleX + paddleWidth;
            const hitPaddleY = ball.y + ballRadius >= paddleY && ball.y - ballRadius <= paddleY + paddleHeight;

            if (withinPaddleX && hitPaddleY) {
                ball.y = paddleY - ballRadius; // 끼임 버그 차단

                const paddleCenterX = paddleX + paddleWidth / 2;
                let relativeX = ball.x - paddleCenterX; 
                let normalizedHit = relativeX / (paddleWidth / 2);
                normalizedHit = Math.max(-1, Math.min(1, normalizedHit)); 

                const maxAngle = 60 * Math.PI / 180;
                const angle = normalizedHit * maxAngle; 

                ball.dx = baseSpeed * Math.sin(angle);
                ball.dy = -baseSpeed * Math.cos(angle); 
                
                playBeep(450, 0.08); 
            }
        }
    }

    // 모든 공이 제거된(목숨을 잃을) 대참사 연산
    if (balls.length === 0) {
        lives--; // 목숨 감소
        items = []; // 기존 아이템 소멸
        missiles = []; // 날아가던 미사일 소멸
        particles = []; // 파티클 소멸
        isPowerUpActive = false; // 파워업 상태 초기화
        isLaserActive = false; // 레이저 상태 초기화
        powerUpTimer = 0;
        laserTimer = 0;
        paddleWidth = basePaddleWidth; // 패들 가로 크기 원상 복귀
        
        if (lives > 0) {
            // 목숨이 남아있으므로 새 공 1개로 출격대기 세팅
            paddleX = (canvas.width - paddleWidth) / 2;
            balls.push({
                x: paddleX + paddleWidth / 2,
                y: paddleY - ballRadius,
                dx: baseSpeed * Math.sin(Math.PI / 4),
                dy: -baseSpeed * Math.cos(Math.PI / 4)
            });
            
            isWaitingToStart = true;
            waitTimerStart = Date.now();
        } else {
            // 목숨을 전부 소진했다면 사망 처리 (시뮬레이션 정지)
            isGameActive = false;
            playGameOverBGM(); // 게임 오버 효과음 재생
        }
    }

    // 실시간 충돌 체크 및 아이템, 미사일 업데이트
    if (balls.length > 0) {
        collisionDetection();
    }
    updateItems();
    updateMissiles();
    updateParticles(); // 파티클 위치 계산 추가
}

// 파티클 업데이트 함수
function updateParticles() {
    if (!isGameActive || isWaitingToStart) return;
    for (let i = particles.length - 1; i >= 0; i--) {
        const p = particles[i];
        p.x += p.dx;
        p.y += p.dy;
        p.dy += 0.2; // 약간의 중력 효과
        p.life -= p.decay;
        
        if (p.life <= 0) {
            particles.splice(i, 1);
        }
    }
}

// 9. 화면을 깨끗하게 지우고 새 배치대로 프레임마다 그리는 렌더링 파트입니다.
function drawPaddle() {
    ctx.save();
    ctx.beginPath();
    if (typeof ctx.roundRect === 'function') {
        ctx.roundRect(paddleX, paddleY, paddleWidth, paddleHeight, 4); // 패들 모서리 둥글게 깎기 (4px)
    } else {
        ctx.rect(paddleX, paddleY, paddleWidth, paddleHeight);
    }
    
    // 파워업 상태일 땐 번쩍이는 민트/하늘색(Cyan) 및 글로우(shadow) 테마 적용!
    if (isPowerUpActive) {
        ctx.fillStyle = '#00E5FF';
        ctx.shadowBlur = 10;
        ctx.shadowColor = '#00E5FF';
    } else {
        ctx.fillStyle = '#E8A33D'; // 기본 주황색 채우기
    }
    
    ctx.fill();
    ctx.closePath();
    ctx.restore();

    // 레이저가 활성화되면 패들 양 끝에 포신 추가
    if (isLaserActive) {
        ctx.save();
        ctx.fillStyle = '#EF4444'; // 빨간색 포신
        ctx.shadowBlur = 8;
        ctx.shadowColor = '#EF4444';
        
        // 왼쪽 포신
        ctx.fillRect(paddleX + 4, paddleY - 6, 6, 6);
        // 오른쪽 포신
        ctx.fillRect(paddleX + paddleWidth - 10, paddleY - 6, 6, 6);
        
        ctx.restore();
    }
}

function drawBalls() {
    for (let i = 0; i < balls.length; i++) {
        const ball = balls[i];
        ctx.beginPath();
        ctx.arc(ball.x, ball.y, ballRadius, 0, Math.PI * 2); // 360도 호를 그려 완벽한 원형 획득
        ctx.fillStyle = '#FFFFFF'; // 흰색 공 지정
        
        // 캔버스 필터를 이용해 네온처럼 빛나는 야광/그림자 효과를 구현합니다. (블러 8px)
        ctx.shadowBlur = 8;
        ctx.shadowColor = '#FFFFFF';
        
        ctx.fill();
        ctx.closePath();
    }
    ctx.shadowBlur = 0; // 필터 무력화
}

function drawBricks() {
    for (let r = 0; r < brickRows; r++) {
        for (let c = 0; c < brickCols; c++) {
            const b = bricks[r][c];
            if (b.status > 0) { // 내구도가 남아있는 벽돌 드로잉
                ctx.beginPath();
                if (typeof ctx.roundRect === 'function') {
                    ctx.roundRect(b.x, b.y, brickWidth, brickHeight, 4); // 벽돌 둥글게 깎기
                } else {
                    ctx.rect(b.x, b.y, brickWidth, brickHeight);
                }
                // 내구도에 따른 색상 동적 적용
                ctx.fillStyle = durabilityColors[b.status] || '#FFFFFF';
                ctx.fill();

                // 벽돌의 입체감과 구분을 위한 1px 두께의 어두운 외곽선 테두리 그리기
                ctx.lineWidth = 1;
                ctx.strokeStyle = 'rgba(15, 23, 42, 0.4)';
                ctx.stroke();
                ctx.closePath();
                
                // 정중앙에 남은 내구도 숫자 표시
                ctx.fillStyle = '#FFFFFF';
                ctx.font = 'bold 14px "Outfit", "Malgun Gothic", sans-serif';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText(b.status.toString(), b.x + brickWidth / 2, b.y + brickHeight / 2);
            }
        }
    }
}

// 미사일 드로잉
function drawMissiles() {
    for (let i = 0; i < missiles.length; i++) {
        const m = missiles[i];
        ctx.save();
        ctx.beginPath();
        if (typeof ctx.roundRect === 'function') {
            ctx.roundRect(m.x, m.y, missileWidth, missileHeight, 2);
        } else {
            ctx.rect(m.x, m.y, missileWidth, missileHeight);
        }
        ctx.fillStyle = '#EF4444'; // 빨간색 미사일
        ctx.shadowBlur = 10;
        ctx.shadowColor = '#EF4444';
        ctx.fill();
        ctx.closePath();
        ctx.restore();
    }
}

// 점수판 텍스트 그리기
function drawScore() {
    ctx.font = "18px 'Malgun Gothic', '맑은 고딕', sans-serif";
    ctx.fillStyle = "#FFFFFF";
    ctx.textAlign = "left"; // 왼쪽 정렬
    ctx.textBaseline = "top"; // 텍스트 기준점은 머리맡(y=25가 천장)
    ctx.fillText("점수 : " + score, 10, 25);
    
    // 최고 점수 표시
    ctx.font = "14px 'Malgun Gothic', '맑은 고딕', sans-serif";
    ctx.fillStyle = "#9CA3AF";
    ctx.fillText("최고 점수 : " + highScore, 10, 48);
}

// 스테이지 텍스트 그리기
function drawStage() {
    ctx.font = "bold 20px 'Malgun Gothic', '맑은 고딕', sans-serif";
    ctx.fillStyle = "#E8A33D";
    ctx.textAlign = "center";
    ctx.textBaseline = "top";
    ctx.fillText("STAGE " + currentStage, canvas.width / 2, 25);
}

// 생명 개수 판 그리기
function drawLives() {
    ctx.font = "18px 'Malgun Gothic', '맑은 고딕', sans-serif";
    ctx.fillStyle = "#FFFFFF";
    ctx.textAlign = "right"; // 오른쪽 정렬
    ctx.textBaseline = "top";
    ctx.fillText("생명 : " + lives, canvas.width - 10, 25);
}

// 화면에 떨어지고 있는 아이템들을 그리는 드로잉 함수
function drawItems() {
    for (let i = 0; i < items.length; i++) {
        const item = items[i];
        ctx.save();
        
        let glowColor = '#00E5FF'; // 기본(expand) 하늘색
        if (item.type === 'multiball') glowColor = '#FFD966'; // 멀티볼 노란색
        else if (item.type === 'laser') glowColor = '#EF4444'; // 레이저 빨간색
        
        ctx.shadowBlur = 8;
        ctx.shadowColor = glowColor;
        
        ctx.beginPath();
        if (typeof ctx.roundRect === 'function') {
            ctx.roundRect(item.x, item.y, itemWidth, itemHeight, 6); // 알약 캡슐 모양
        } else {
            ctx.rect(item.x, item.y, itemWidth, itemHeight);
        }
        ctx.fillStyle = glowColor;
        ctx.fill();
        
        ctx.lineWidth = 1;
        ctx.strokeStyle = '#FFFFFF'; // 흰색 테두리
        ctx.stroke();
        
        ctx.closePath();
        ctx.restore();
    }
}

// 화면 하단 구석(패들 아래 여백)에 타이머를 표시하여 벽돌 시야를 가리지 않게 조정합니다.
function drawTimers() {
    const yPos = canvas.height - 15; // 화면 맨 아래쪽 여백
    
    if (isPowerUpActive) {
        const timeLeft = Math.max(0, (powerUpTimer - Date.now()) / 1000);
        ctx.font = "bold 16px 'Malgun Gothic', '맑은 고딕', sans-serif";
        ctx.fillStyle = "#00E5FF";
        ctx.textAlign = "left";
        ctx.textBaseline = "bottom";
        ctx.fillText("패들 확장: " + timeLeft.toFixed(1) + "초", 15, yPos);
    }
    
    if (isLaserActive) {
        const timeLeft = Math.max(0, (laserTimer - Date.now()) / 1000);
        ctx.font = "bold 16px 'Malgun Gothic', '맑은 고딕', sans-serif";
        ctx.fillStyle = "#EF4444";
        ctx.textAlign = "right";
        ctx.textBaseline = "bottom";
        ctx.fillText("레이저 건: " + timeLeft.toFixed(1) + "초", canvas.width - 15, yPos);
    }
}

// 파티클 드로잉
function drawParticles() {
    for (let i = 0; i < particles.length; i++) {
        const p = particles[i];
        ctx.save();
        ctx.globalAlpha = Math.max(0, p.life); // 수명에 따라 투명해짐
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
        ctx.fillStyle = p.color;
        
        // 파편 글로우 효과
        ctx.shadowBlur = 6;
        ctx.shadowColor = p.color;
        
        ctx.fill();
        ctx.closePath();
        ctx.restore();
    }
}

// 전체 도화지를 싹 지우고 모든 레이어를 순서대로 겹쳐서 그리는 메인 렌더 스튜디오 함수입니다.
function draw() {
    // 1) 이전 화면의 흔적을 덮어버릴 순수한 검은색 도화지 한 장 깔기
    ctx.fillStyle = '#000000';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // 2) (제거됨: 게임 화면 가운데 BRICK BREAKER 워터마크 텍스트는 유저 요청으로 숨김 처리)

    // 3) 게임의 실질적인 3요소 (벽돌 -> 패들 -> 다중 공)와 미사일, 아이템, 정보 요소 그리기
    drawBricks();
    drawPaddle();
    drawBalls();
    drawMissiles(); // 발사된 미사일 그리기
    drawItems(); // 파워업 아이템 리스트 그리기
    drawParticles(); // 흩뿌려지는 파편 그리기
    drawScore();
    drawLives();
    drawStage(); // 현재 스테이지 텍스트 그리기
    drawTimers(); // 활성화된 아이템 타이머 그리기

    // 4) 론칭 전 초기 시작 대기화면 레이어 오버레이
    if (!isGameLaunched) {
        ctx.fillStyle = 'rgba(15, 23, 42, 0.85)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        ctx.fillStyle = '#FFFFFF';
        ctx.font = '800 48px "Outfit", sans-serif';
        ctx.fillText('BREAK BREAKER', canvas.width / 2, canvas.height / 2 - 40);

        ctx.font = '600 24px "Outfit", sans-serif';
        ctx.fillText('by Sungmin Song', canvas.width / 2, canvas.height / 2 - 5);

        ctx.fillStyle = '#E8A33D';
        ctx.font = "600 20px 'Malgun Gothic', '맑은 고딕', sans-serif";
        ctx.fillText('PRESS SPACE OR TOUCH TO START', canvas.width / 2, canvas.height / 2 + 35);
        return; // 오버레이 뒤에 게임 오버 등이 덧씌워지지 않게 조기 종료
    }

    // 5) 생명을 하나 날리고 출격하기 직전 1초간 카운트다운 레이어
    if (isGameActive && isWaitingToStart) {
        ctx.fillStyle = 'rgba(15, 23, 42, 0.6)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.fillStyle = '#E8A33D';
        ctx.font = '800 48px "Outfit", sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('READY...', canvas.width / 2, canvas.height / 2);
    }

    // 6) 게임 오버 혹은 게임 클리어(승리) 시의 결말 블랙아웃 화면 레이어
    if (!isGameActive && !isWaitingToStart) {
        ctx.fillStyle = 'rgba(15, 23, 42, 0.85)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        if (isGameWon) {
            ctx.fillStyle = '#2E9E5B'; // 승리는 초록색 테마
            ctx.font = '800 48px "Outfit", sans-serif';
            ctx.fillText('YOU WIN!', canvas.width / 2, canvas.height / 2 - 40);
        } else {
            ctx.fillStyle = '#EF4444'; // 패배는 빨간색 테마
            ctx.font = '800 48px "Outfit", sans-serif';
            ctx.fillText('GAME OVER', canvas.width / 2, canvas.height / 2 - 40);
        }

        // 최종 점수 표시판
        ctx.fillStyle = '#FFFFFF';
        ctx.font = "600 24px 'Malgun Gothic', '맑은 고딕', sans-serif";
        ctx.fillText('최종점수: ' + score, canvas.width / 2, canvas.height / 2 + 15);

        // 스페이스 재설정 문구
        ctx.fillStyle = '#9CA3AF';
        ctx.font = "400 16px 'Malgun Gothic', '맑은 고딕', sans-serif";
        ctx.fillText('스페이스 바로 재시작', canvas.width / 2, canvas.height / 2 + 65);
    }
}

// 10. 매끄러운 모션 업데이트와 드로잉을 무한 반복 실행시키는 게임 메인 루프입니다.
function gameLoop() {
    update(); // 물리 법칙 갱신
    draw(); // 지우고 다시 그리기
    
    // 브라우저의 화면 주사율(일반적으로 60Hz)에 맞춰 다음 프레임 그리기를 자동 대기/실행해주는 강력한 예약 도구입니다.
    requestAnimationFrame(gameLoop);
}

// 브라우저의 이쁜 구글 Outfit 글꼴 로딩이 완벽하게 완료된 후 게임 메인 루프를 활성화하여 글꼴 깜빡임 현상을 방지합니다.
if (document.fonts) {
    document.fonts.ready.then(() => {
        requestAnimationFrame(gameLoop);
    });
} else {
    requestAnimationFrame(gameLoop);
}
