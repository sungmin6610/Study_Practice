use semicon_equipdb;
-- 예제 1. 공정운영팀은 어떤 장비를 사용하는가?

-- 예제 2. 유지보수는 어떤 장비에 대해 이루어졌는가?
-- 1. 누가 등장하는가? 장비
-- 2. 무엇을 했는가?	유지보수
-- 3. 그 행동은 어디에 기록되는가?	MaintenanceLog
-- 4. 다른 정보는 어떻게 연결하는가? 장비정보 -> Equipment
select m.maintenance_date, e.model_name, m.maintenance_type
from maintenancelog m
join equipment e on m.equipment_id = e.equipment_id
order by maintenance_date;

-- 예제 3. 현재 maintenance 상태인 장비는 어떤 유지보수 기록과 관련이 있는가?
-- 1. 누가 등장하는가? 장비
-- 2. 무엇을 했는가?	유지보수
-- 3. 그 행동은 어디에 기록되는가?	MaintenanceLog
-- 4. 다른 정보는 어떻게 연결하는가? 장비정보 -> Equipment
select *
from maintenancelog m
join equipment e on m.equipment_id = e.equipment_id
where status = 'maintenance';


-- 문제 1 품질분석팀은 어떤 장비를 사용하는가?
-- 1. 누가 등장하는가?	품질분석팀
-- 2. 무엇을 했는가? 	장비를 사용
-- 3. 그 행동은 어디에 기록되는가?	UsageLog
-- 4. 다른 정보는 어떻게 연결하는가?
	-- 3번에서 알 수 없는 정보는 무엇인가?	장비, 부서
    -- 그럼 그건 어디에 있나? Equipment, Employee, Department




-- 문제 2 어떤 장비에서 이상이 발생했는가?
-- 1. 누가 등장하는가?		장비
-- 2. 무엇을 했는가?		이상이 발생
-- 3. 그 행동은 어디에 기록되는가?	UsageLog
-- 4. 다른 정보는 어떻게 연결하는가?
	-- 3번에서 알 수 없는 정보는 무엇인가?	 장비 정보
    -- 그럼 그건 어디에 있나? Equipment
select u.use_date, e.model_name, u.issue_report
from usagelog u
join equipment e on u.equipment_id = e.equipment_id
where u.issue_report is not null;




-- 문제 3 이상이 발생한 장비는 이후 유지보수가 있었는가?
-- 1. 누가 등장하는가? 장비 
-- 2. 무엇을 했는가?	유지보수 당함, 이상이 발생
-- 3. 그 행동은 어디에 기록되는가?	MaintenanceLog, UsageLog
-- 4. 다른 정보는 어떻게 연결하는가?
	-- 3번에서 알 수 없는 정보는 무엇인가?		X
    -- 그럼 그건 어디에 있나? 				X
	-- equipment_id //
select *
from usagelog u
join maintenancelog m on u.equipment_id = m.equipment_id
where u.issue_report is not null
and u.use_date < m.maintenance_date;

-- 문제 4 유지보수 작업은 어떤 엔지니어가 수행했는가?
-- 1. 누가 등장하는가?		엔지니어
-- 2. 무엇을 했는가?		유지보수 작업
-- 3. 그 행동은 어디에 기록되는가?	MaintenanceLog
-- 4. 다른 정보는 어떻게 연결하는가?
	-- 3번에서 알 수 없는 정보는 무엇인가?			엔지니어 이름(정보)
    -- 그럼 그건 어디에 있나? 	Employee
select distinct e.name
from maintenancelog m
join employee e on m.engineer_id = e.employee_id;


-- 문제 5 특정 장비(예: 102번)는 어떤 과정을 거쳐 현재 상태에 이르렀는가?
-- 1. 누가 등장하는가?		102번 장비
-- 2. 무엇을 했는가?		사용됨, 이상이 발생, 유지보수 당함
-- 3. 그 행동은 어디에 기록되는가?	UsageLog, MaintenanceLog
-- 4. 다른 정보는 어떻게 연결하는가?
	-- 3번에서 알 수 없는 정보는 무엇인가?		장비 이름, 장비 상태
    -- 그럼 그건 어디에 있나? 	Equipment