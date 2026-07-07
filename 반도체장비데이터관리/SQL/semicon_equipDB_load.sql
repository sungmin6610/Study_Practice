-- 반도체 장비 실습용 DB: 마당서점DB 구조 기반

-- 0. 데이터베이스 생성
CREATE DATABASE semicon_equipDB;
USE semicon_equipDB;

DROP TABLE if exists UsageLog;
DROP TABLE if exists EquipmentUser;
DROP TABLE if exists Equipment;

-- 1. 장비 사용자 테이블 (기존: Customer)
CREATE TABLE EquipmentUser (
    user_id INT PRIMARY KEY,
    name VARCHAR(20),
    department VARCHAR(20),
    contact VARCHAR(50)
);

-- 2. 장비 테이블 (기존: Book)
CREATE TABLE Equipment (
    equipment_id INT PRIMARY KEY,
    model_name VARCHAR(50),
    install_date DATE,
    status VARCHAR(20)
);

-- 3. 장비 사용 기록 테이블 (기존: Orders)
CREATE TABLE UsageLog (
    log_id INT PRIMARY KEY,
    user_id INT,
    equipment_id INT,
    use_date DATE,
    issue_report TEXT,
    FOREIGN KEY (user_id) REFERENCES EquipmentUser(user_id),
    FOREIGN KEY (equipment_id) REFERENCES Equipment(equipment_id)
);

-- 샘플 데이터 삽입

-- EquipmentUser 데이터
INSERT INTO EquipmentUser VALUES
(1, '김지훈', '품질팀', '010-1234-5678'),
(2, '박서윤', '제조팀', '010-2345-6789'),
(3, '이준호', '품질팀', '010-3456-7890'),
(4, '김민아', '개발팀', '010-4567-8901'),
(5, '정예린', '연구팀', '010-5678-9012');

-- Equipment 데이터
INSERT INTO Equipment VALUES
(101, 'ETCH-A100', '2021-05-10', 'active'),
(102, 'CMP-X200', '2023-11-23', 'maintenance'),
(103, 'LITHO-Z300', '2022-07-15', 'active'),
(104, 'CVD-B500', '2019-03-12', 'retired'),
(105, 'DIFF-Y700', '2023-01-20', 'active');

-- UsageLog 데이터
INSERT INTO UsageLog VALUES
(1, 1, 101, '2024-03-01', '챔버 압력 이상'),
(2, 2, 103, '2024-03-02', NULL),
(3, 3, 102, '2024-03-05', '연마 균일도 문제'),
(4, 1, 104, '2024-03-07', NULL),
(5, 4, 101, '2024-03-08', '이물 감지 센서 오작동'),
(6, 5, 105, '2024-03-10', NULL),
(7, 3, 103, '2024-03-11', NULL),
(8, 2, 105, '2024-03-12', '예열 지연'),
(9, 1, 102, '2024-03-13', NULL),
(10, 4, 103, '2024-03-14', '패턴 비정상'),
(11, 5, 101, '2024-03-15', NULL);

-- 데이터 관계 요약:
-- EquipmentUser (1) : UsageLog (N)
-- Equipment      (1) : UsageLog (N)
-- → 마당서점의 Customer-Orders-Book 구조와 완전히 동일함

select * from Equipment;
select * from EquipmentUser;
select * from Usagelog;

