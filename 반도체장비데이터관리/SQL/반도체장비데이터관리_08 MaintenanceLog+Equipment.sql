-- =========================================
-- MaintenanceLog + Equipment 실습문제
-- =========================================

use semicon_equipdb;
-- 문제 1
-- 유지보수일자, 장비 모델명, 담당 엔지니어 이름을 함께 조회하세요.
SELECT m.maintenance_date, e.model_name, m.engineer_name
FROM maintenancelog AS m
JOIN equipment AS e ON m.equipment_id = e.equipment_id;

-- 문제 2
-- maintenance_type이 '정기점검'인 기록에 대해
-- 유지보수일자, 장비 모델명, 담당 엔지니어 이름을 조회하세요.
SELECT m.maintenance_date, e.model_name, m.engineer_name
FROM maintenancelog AS m
JOIN equipment AS e ON m.equipment_id = e.equipment_id
WHERE m.maintenance_type = '정기점검';

-- 문제 3
-- 각 엔지니어별 유지보수 건수를 조회하세요.
-- 엔지니어 이름과 유지보수 건수를 출력하세요.
select engineer_name, count(*)
from maintenancelog
group by engineer_name;

-- 문제 4
-- 각 장비별 유지보수 건수를 조회하세요.
-- 장비 모델명과 유지보수 건수를 출력하세요.
SELECT e.model_name, COUNT(*)
FROM maintenancelog AS m
JOIN equipment AS e ON m.equipment_id = e.equipment_id
GROUP BY e.model_name;

-- 문제 5
-- 유지보수 기록이 2건 이상인 엔지니어의 이름과 유지보수 건수를 조회하세요.
select engineer_name, count(*)
from maintenancelog
group by engineer_name
having count(*) >= 2;


-- 문제 6
-- 각 장비별 가장 최근 유지보수 날짜를 조회하세요.
-- 장비 모델명과 가장 최근 유지보수 날짜를 출력하세요.
select model_name, max(maintenance_date)
from maintenancelog as m
join equipment as e on m.equipment_id = e.equipment_id
group by model_name;

-- 문제 7
-- 유지보수가 한 번도 수행되지 않은 장비의 모델명을 조회하세요.
select model_name
from equipment as e
left join maintenancelog as m on e.equipment_id = m.equipment_id
where m.equipment_id is null;

-- 문제 8
-- 각 장비별 유지보수 건수를 조회하세요.
-- 유지보수가 없는 장비도 포함하여 출력하세요.
select e.model_name, count(*)
from equipment as e
join maintenancelog as m on e.equipment_id = m.equipment_id
group by e.model_name;


