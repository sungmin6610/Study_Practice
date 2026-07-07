-- =========================================
-- JOIN 연습문제 (압축 버전)
-- EquipmentUser / Equipment / UsageLog
-- =========================================

use semicon_equipDB;
-- =========================================
-- ① 힌트형 문제 (6문제)
-- =========================================

-- 1. 장비 이상 이력이 보고된 사용자를 파악하려고 한다.
-- 사용자 이름, 사용일자, issue_report를 조회하시오.
-- (EquipmentUser, UsageLog)
select name, use_date, issue_report
from equipmentuser
join usagelog
on equipmentuser.user_id = usagelog.user_id
where usagelog.issue_report is not null;

-- 2. 어떤 장비에서 어떤 문제가 발생했는지 확인하려고 한다.
-- 장비 모델명, 사용일자, issue_report를 조회하시오.
-- (Equipment, UsageLog)
select equipment.model_name, usagelog.use_date, usagelog.issue_report
from equipment 
join usagelog
on equipment.equipment_id = usagelog.equipment_id
where usagelog.issue_report is not null;

-- 3. 현재 사용 가능한 장비의 실제 사용 기록만 따로 보려고 한다.
-- 장비 모델명, 사용일자를 조회하시오.
-- 단, status가 'active'인 장비만 조회하시오.
-- (Equipment, UsageLog)
select equipment.model_name, usagelog.use_date
from equipment
join usagelog
on equipment.equipment_id = usagelog.equipment_id
where equipment.status = 'active';

-- 4. 품질팀 사용자의 장비 사용 기록만 별도로 점검하려고 한다.
-- 사용자 이름, 부서, 사용일자를 조회하시오.
-- 단, department가 '품질팀'인 사용자만 조회하시오.
-- (EquipmentUser, UsageLog)
select name, department, use_date
from equipmentuser
join usagelog
on equipmentuser.user_id = usagelog.user_id
where equipmentuser.department = '품질팀';

-- 5. 문제 발생 기록을 최신 순서대로 검토하려고 한다.
-- 장비 모델명, issue_report, 사용일자를 조회하되,
-- issue_report가 있는 경우만, 사용일자 내림차순으로 정렬하시오.
-- (Equipment, UsageLog)
select model_name, issue_report, use_date
from equipment
join usagelog
on equipment.equipment_id = usagelog.equipment_id
where usagelog.issue_report is not null
order by usagelog.use_date desc;

-- 6. 연락이 필요한 사용자를 빠르게 확인하기 위해 사용 기록과 연락처를 함께 보려고 한다.
-- 사용자 이름, contact, 사용일자를 조회하시오.
-- (EquipmentUser, UsageLog)
select name, contact, use_date, equipment.model_name
from equipmentuser
join usagelog
on equipmentuser.user_id = usagelog.user_id
join equipment
on usagelog.equipment_id = equipment.equipment_id;


-- =========================================
-- ② 요구사항 해석형 문제 (6문제)
-- =========================================

-- 7. “3월 중에 장비에 문제가 있었던 날만 따로 보고 싶어요.”
-- 사용자 이름, 장비 모델명, 사용일자를 조회하시오.
select name, model_name, use_date
from usagelog
join equipment
on usagelog.equipment_id = equipment.equipment_id
join equipmentuser
on usagelog.user_id = equipmentuser.user_id
where usagelog.use_date between '2024-03-01' and '2024-03-31' and usagelog.issue_report is not null ;

-- 8. “문제 없는 정상 사용 기록만 날짜순으로 보고 싶어요.”
-- 사용자 이름, 장비 모델명, 사용일자를 조회하되, 사용일자 오름차순으로 정렬하시오.
select name, model_name, use_date;


-- 9. “최근에 누가 어떤 장비를 썼는지 최신 것부터 보고 싶어요.”
-- 사용자 이름, 장비 모델명, 사용일자를 조회하되, 최신 순으로 정렬하시오.
select name, model_name, use_date
from equipmentuser
join usagelog
on equipmentuser.user_id = usagelog.user_id
join equipment
on equipment.equipment_id = usagelog.equipment_id
order by use_date desc;

-- 10. “개발팀 사람이 사용한 장비 기록만 따로 뽑아주세요.”
-- 개발팀 사용자의 이름, 장비 모델명, 사용일자를 조회하시오.
select name, model_name, use_date
from usagelog
join equipment
on usagelog.equipment_id = equipment.equipment_id
join equipmentuser
on usagelog.user_id = equipmentuser.user_id
where department = '개발팀';

-- 11. “2023년 이후 설치된 장비가 실제로 사용된 기록만 보고 싶어요.”
-- 장비 모델명, 설치일자, 사용일자를 조회하시오.
select equipment.model_name, install_date, usagelog.use_date
from usagelog
join equipment 
on usagelog.equipment_id = equipment.equipment_id
join equipmentuser 
on usagelog.user_id = equipmentuser.user_id
where install_date >= '2023-01-01';

-- 12. “사용자한테 바로 연락해야 할 수도 있어서, 사용 기록 볼 때 연락처도 같이 나오면 좋겠어요.”
-- 사용자 이름, 연락처, 장비 ID, 사용일자를 조회하시오.
select name, contact, equipment_id, use_date
from 
