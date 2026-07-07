-- =========================================
-- semiconDB JOIN 실습문제
-- =========================================
use semicon_equipdb;

-- [INNER JOIN vs LEFT JOIN 비교 - 사용자 기준]

-- 문제 1
-- 사용자 이름과 사용 로그 번호를 조회하세요.
-- 사용 기록이 있는 사용자만 조회되도록 작성하세요.
select u.name, l.log_id
from equipmentuser as u
inner join usagelog as l
on u.user_id = l.user_id;

-- 문제 2
-- 모든 사용자의 이름과 사용 로그 번호를 조회하세요.
-- 사용 기록이 없는 사용자도 포함되도록 작성하세요.
select u.name, l.log_id
from equipmentuser as u
left join usagelog as l
on u.user_id = l.user_id;

-- 문제 3
-- 문제 1과 문제 2의 결과를 비교하여,
-- 어떤 사용자가 LEFT JOIN에서만 나타나는지 확인하세요.



-- [사용자 기준 LEFT JOIN 활용]

-- 문제 4
-- 모든 사용자의 사용자번호, 이름, 사용일자를 조회하세요.
-- 사용 기록이 없는 사용자도 포함하세요.
select u.user_id, u.name, l.use_date
from equipmentuser as u
left join usagelog as l
on u.user_id = u.user_id;

-- 문제 5
-- 사용 기록이 없는 사용자만 조회하세요.
-- (사용자번호, 이름)
select u.user_id, u.name
from equipmentuser as u
left join usagelog as l
on u.user_id = l.user_id
where log_id is null;


-- [장비 기준 INNER JOIN vs LEFT JOIN]

-- 문제 6
-- 장비 모델명과 사용 로그 번호를 조회하세요.
-- 사용된 장비만 조회되도록 작성하세요.
select e.model_name, u.log_id
from Equipment as e
inner join UsageLog as u 
on e.equipment_id = u.equipment_id;

-- 문제 7
-- 모든 장비의 장비번호, 모델명, 사용 로그 번호를 조회하세요.
-- 사용된 적 없는 장비도 포함하세요.
select e.equipment_id, e.model_name, u.log_id
from Equipment as e
left join UsageLog as u 
on e.equipment_id = u.equipment_id;

-- 문제 8
-- 한 번도 사용되지 않은 장비만 조회하세요.
-- (장비번호, 모델명)
select e.equipment_id, e.model_name
from Equipment as e
left join UsageLog as u 
on e.equipment_id = u.equipment_id
where u.log_id is null;


-- [JOIN 확장 - 사용자 + 장비]

-- 문제 9
-- 사용자 이름, 장비 모델명, 사용일자를 조회하세요.
-- 문제 9
-- 사용자 이름, 장비 모델명, 사용일자를 조회하세요.
select eu.name, e.model_name, u.use_date
from UsageLog as u
inner join EquipmentUser as eu 
on u.user_id = eu.user_id
inner join Equipment as e 
on u.equipment_id = e.equipment_id;

-- 문제 10
-- 모든 사용자의 이름과 장비 모델명을 조회하세요.
-- 사용 기록이 없는 사용자도 포함하세요.
-- 문제 10
-- 모든 사용자의 이름과 장비 모델명을 조회하세요. (사용 기록 없는 사용자 포함)
select eu.name, e.model_name
from EquipmentUser as eu
left join UsageLog as u 
on eu.user_id = u.user_id
left join Equipment as e 
on u.equipment_id = e.equipment_id;