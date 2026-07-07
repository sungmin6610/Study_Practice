-- =========================================================
-- SemiconDB 구조 리팩터링 실습
-- 내용:
-- 1) EquipmentUser -> Employee 테이블명 변경
-- 2) UsageLog.user_id -> employee_id 컬럼명 변경
-- 3) MaintenanceLog.engineer_name -> engineer_id로 변경
-- 4) Department.manager_name -> manager_id로 변경
-- =========================================================


-- =========================================================
-- 0. 현재 상태 확인
-- =========================================================
SELECT * FROM Department;
SELECT * FROM EquipmentUser;
SELECT * FROM Equipment;
SELECT * FROM UsageLog;
SELECT * FROM MaintenanceLog;


-- =========================================================
-- 1. EquipmentUser -> Employee 테이블명 변경
-- =========================================================



-- =========================================================
-- 2. Employee의 user_id -> employee_id로 컬럼명 변경
-- =========================================================



-- =========================================================
-- 3. UsageLog의 user_id -> employee_id로 컬럼명 변경
-- =========================================================



-- =========================================================
-- 4. MaintenanceLog: engineer_name -> engineer_id
-- =========================================================

-- 4-1. 새 컬럼 추가


-- 4-2. 이름 기준으로 engineer_id 매핑

-- 4-3. 매핑 결과 확인


-- 4-4. 매핑되지 않은 데이터 확인


-- 4-5. 모든 행이 정상적으로 매핑된 뒤 NOT NULL 설정


-- 4-6. 외래키 추가


-- 4-7. 기존 engineer_name 컬럼 삭제



-- =========================================================
-- 5. Department: manager_name -> manager_id
-- =========================================================

-- 5-1. 새 컬럼 추가


-- 5-2. 이름 기준으로 manager_id 매핑


-- 5-3. 매핑 결과 확인


-- 5-4. 매핑되지 않은 데이터 확인


-- 5-5. manager_id가 모두 채워졌다면 외래키 추가


-- 5-6. 기존 manager_name 컬럼 삭제


-- =========================================================
-- 6. 최종 구조 확인
-- =========================================================
DESC Department;
DESC Employee;
DESC Equipment;
DESC UsageLog;
DESC MaintenanceLog;

SHOW CREATE TABLE Department;
SHOW CREATE TABLE Employee;
SHOW CREATE TABLE UsageLog;
SHOW CREATE TABLE MaintenanceLog;


-- =========================================================
-- 7. 최종 데이터 확인
-- =========================================================
SELECT * FROM Department;
SELECT * FROM Employee;
SELECT * FROM UsageLog;
SELECT * FROM MaintenanceLog;