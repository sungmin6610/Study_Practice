-- =========================================
-- MaintenanceLog - 쿼리 실습 
-- =========================================

use semicon_equipdb;
-- 문제 1
-- MaintenanceLog 테이블의 모든 데이터를 조회하세요.

select *
from MaintenanceLog; 

-- 문제 2
-- equipment_id가 101인 유지보수 기록을 조회하세요.

select *
from maintenancelog
where equipment_id = '101';

-- 문제 3
-- engineer_name이 '박수현'인 유지보수 기록을 조회하세요.
select *
from maintenancelog
where engineer_name = '박수현';

-- 문제 4
-- maintenance_type이 '정기점검'인 기록만 조회하세요.
select *
from maintenancelog
where maintenance_type = '정기점검';

-- 문제 5
-- 2024-03-05 이후의 유지보수 기록을 조회하세요.
select *
from maintenancelog
where maintenance_date >= '2024-03-05';

-- 문제 6
-- 유지보수 날짜를 최신 순으로 정렬하여 조회하세요.
select *
from maintenancelog
order by maintenance_date desc;

-- 문제 7
-- equipment_id가 102이고 maintenance_type이 '정기점검'인 기록을 조회하세요.
select *
from maintenancelog
where equipment_id = '102' and maintenance_type = '정기점검';

-- 문제 8
-- '수리'가 포함된 유지보수 유형만 조회하세요.
select *
from maintenancelog
where maintenance_type like '%수리%';