-- =========================================================
-- Process Sensor Data DB 생성 실습 
-- =========================================================


DROP DATABASE IF EXISTS ProcessSensorDB;
CREATE DATABASE ProcessSensorDB;
USE ProcessSensorDB;


-- =========================================================
-- 1. ProcessStep: 공정 단계
-- =========================================================
CREATE TABLE ProcessStep (
    step_id INT PRIMARY KEY,
    step_name VARCHAR(50) NOT NULL,
    process_group VARCHAR(50) NOT NULL
);

INSERT INTO ProcessStep VALUES
(1, '산화막 증착', 'Deposition'),
(2, '식각 공정', 'Etch'),
(3, '세정 공정', 'Cleaning'),
(4, '검사 공정', 'Inspection');


-- =========================================================
-- 2. Lot: 생산 단위
-- =========================================================
CREATE TABLE Lot (
    lot_id VARCHAR(10) PRIMARY KEY,
    product_name VARCHAR(50) NOT NULL,
    wafer_qty INT NOT NULL,
    lot_status VARCHAR(20) NOT NULL
);

INSERT INTO Lot VALUES
('L001', 'DRAM-A', 25, 'processing'),
('L002', 'DRAM-A', 25, 'completed'),
('L003', 'NAND-B', 25, 'warning');


-- =========================================================
-- 3. Equipment: 장비
-- =========================================================
CREATE TABLE Equipment (
    equipment_id INT PRIMARY KEY,
    model_name VARCHAR(50) NOT NULL,
    equipment_type VARCHAR(50) NOT NULL,
    status VARCHAR(20) NOT NULL
);

INSERT INTO Equipment VALUES
(101, 'CVD-B200', 'Deposition', 'active'),
(102, 'ETCH-A100', 'Etch', 'active'),
(103, 'CLEAN-D300', 'Cleaning', 'active'),
(104, 'INSPECT-E500', 'Inspection', 'active');


-- =========================================================
-- 4. RunHistory: 실제 공정 실행 이력
-- =========================================================
CREATE TABLE RunHistory (
    run_id INT PRIMARY KEY,
    lot_id VARCHAR(10) NOT NULL,
    equipment_id INT NOT NULL,
    step_id INT NOT NULL,
    start_time DATETIME NOT NULL,
    end_time DATETIME NOT NULL,
    run_status VARCHAR(20) NOT NULL,
    FOREIGN KEY (lot_id) REFERENCES Lot(lot_id),
    FOREIGN KEY (equipment_id) REFERENCES Equipment(equipment_id),
    FOREIGN KEY (step_id) REFERENCES ProcessStep(step_id)
);

INSERT INTO RunHistory VALUES
(1, 'L001', 101, 1, '2024-03-15 09:00:00', '2024-03-15 09:30:00', 'completed'),
(2, 'L001', 102, 2, '2024-03-15 10:00:00', '2024-03-15 10:25:00', 'completed'),
(3, 'L001', 103, 3, '2024-03-15 10:40:00', '2024-03-15 10:55:00', 'completed'),

(4, 'L002', 101, 1, '2024-03-15 11:10:00', '2024-03-15 11:45:00', 'completed'),
(5, 'L002', 102, 2, '2024-03-15 12:10:00', '2024-03-15 12:35:00', 'completed'),
(6, 'L002', 104, 4, '2024-03-15 13:00:00', '2024-03-15 13:20:00', 'completed'),

(7, 'L003', 101, 1, '2024-03-15 14:00:00', '2024-03-15 14:35:00', 'warning'),
(8, 'L003', 102, 2, '2024-03-15 15:00:00', '2024-03-15 15:30:00', 'warning');


-- =========================================================
-- 5. Sensor: 센서 정의
-- =========================================================
CREATE TABLE Sensor (
    sensor_id INT PRIMARY KEY,
    equipment_id INT NOT NULL,
    sensor_name VARCHAR(50) NOT NULL,
    unit VARCHAR(20) NOT NULL,
    normal_min DECIMAL(10,2) NOT NULL,
    normal_max DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (equipment_id) REFERENCES Equipment(equipment_id)
);

INSERT INTO Sensor VALUES
(1, 101, 'chamber_temp', 'C', 390.00, 410.00),
(2, 101, 'chamber_pressure', 'Torr', 1.80, 2.20),

(3, 102, 'rf_power', 'W', 480.00, 520.00),
(4, 102, 'gas_flow', 'sccm', 95.00, 105.00),

(5, 103, 'cleaning_flow', 'L/min', 18.00, 22.00),
(6, 103, 'chemical_temp', 'C', 23.00, 27.00),

(7, 104, 'defect_count', 'count', 0.00, 5.00),
(8, 104, 'inspection_temp', 'C', 22.00, 28.00);


-- =========================================================
-- 6. SensorMeasurement: 실제 센서 측정값
-- =========================================================
CREATE TABLE SensorMeasurement (
    measurement_id INT PRIMARY KEY,
    run_id INT NOT NULL,
    sensor_id INT NOT NULL,
    measured_at DATETIME NOT NULL,
    measured_value DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (run_id) REFERENCES RunHistory(run_id),
    FOREIGN KEY (sensor_id) REFERENCES Sensor(sensor_id)
);

INSERT INTO SensorMeasurement VALUES
-- Run 1: L001, CVD-B200, 산화막 증착, 정상
(1, 1, 1, '2024-03-15 09:05:00', 398.00),
(2, 1, 1, '2024-03-15 09:15:00', 402.00),
(3, 1, 1, '2024-03-15 09:25:00', 405.00),
(4, 1, 2, '2024-03-15 09:05:00', 1.95),
(5, 1, 2, '2024-03-15 09:15:00', 2.00),
(6, 1, 2, '2024-03-15 09:25:00', 2.05),

-- Run 2: L001, ETCH-A100, 식각, 정상
(7, 2, 3, '2024-03-15 10:05:00', 500.00),
(8, 2, 3, '2024-03-15 10:15:00', 508.00),
(9, 2, 4, '2024-03-15 10:05:00', 100.00),
(10, 2, 4, '2024-03-15 10:15:00', 102.00),

-- Run 3: L001, CLEAN-D300, 세정, 정상
(11, 3, 5, '2024-03-15 10:45:00', 20.00),
(12, 3, 5, '2024-03-15 10:50:00', 21.00),
(13, 3, 6, '2024-03-15 10:45:00', 25.00),
(14, 3, 6, '2024-03-15 10:50:00', 24.50),

-- Run 4: L002, CVD-B200, 산화막 증착, 정상
(15, 4, 1, '2024-03-15 11:15:00', 399.00),
(16, 4, 1, '2024-03-15 11:30:00', 401.00),
(17, 4, 2, '2024-03-15 11:15:00', 1.90),
(18, 4, 2, '2024-03-15 11:30:00', 2.10),

-- Run 5: L002, ETCH-A100, 식각, 정상
(19, 5, 3, '2024-03-15 12:15:00', 495.00),
(20, 5, 3, '2024-03-15 12:25:00', 505.00),
(21, 5, 4, '2024-03-15 12:15:00', 98.00),
(22, 5, 4, '2024-03-15 12:25:00', 101.00),

-- Run 6: L002, INSPECT-E500, 검사, 정상
(23, 6, 7, '2024-03-15 13:05:00', 3.00),
(24, 6, 7, '2024-03-15 13:15:00', 4.00),
(25, 6, 8, '2024-03-15 13:05:00', 25.00),
(26, 6, 8, '2024-03-15 13:15:00', 26.00),

-- Run 7: L003, CVD-B200, 산화막 증착, warning
-- chamber_temp가 정상 범위 초과
(27, 7, 1, '2024-03-15 14:05:00', 408.00),
(28, 7, 1, '2024-03-15 14:20:00', 416.00),
(29, 7, 1, '2024-03-15 14:30:00', 418.00),
(30, 7, 2, '2024-03-15 14:05:00', 2.05),
(31, 7, 2, '2024-03-15 14:20:00', 2.10),
(32, 7, 2, '2024-03-15 14:30:00', 2.15),

-- Run 8: L003, ETCH-A100, 식각, warning
-- rf_power와 gas_flow가 정상 범위에서 벗어남
(33, 8, 3, '2024-03-15 15:05:00', 525.00),
(34, 8, 3, '2024-03-15 15:20:00', 530.00),
(35, 8, 4, '2024-03-15 15:05:00', 92.00),
(36, 8, 4, '2024-03-15 15:20:00', 90.00);

select * from processStep;
select * from Lot;
select * from equipment;
select * from sensor;
select * from runhistory;
select * from sensormeasurement;