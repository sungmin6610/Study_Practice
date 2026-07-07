-- =========================================
-- GROUP BY 실습 문제
-- =========================================
use semicon_equipDB;
-- [1] GROUP BY 단독

-- Q1. EquipmentUser 테이블에서 부서 목록을 조회하시오.
select department
from equipmentuser
group by department;

-- Q2. Equipment 테이블에서 장비 상태 목록을 조회하시오.
select status
from equipment
group by status;

-- Q3. UsageLog 테이블에서 사용 이력이 있는 장비 ID 목록을 조회하시오.
select equipment_id
from usagelog
group by equipment_id;

-- Q4. UsageLog 테이블에서 장비를 사용한 사용자 ID 목록을 조회하시오.
select user_id
from usagelog
group by user_id;

-- Q5. UsageLog 테이블에서 사용 기록이 존재하는 날짜 목록을 조회하시오.
select use_date
from usagelog
group by use_date;

-- =========================================
-- [2] GROUP BY + WHERE
-- =========================================

-- Q6. retired 상태가 아닌 장비들의 상태 목록을 조회하시오.
select status
from equipment
where status <> 'retired'
group by status;

-- Q7. 2024년 3월에 실제로 사용된 장비 ID 목록을 조회하시오.
select equipment_id
from usagelog
where use_date between '2024-03-01' and '2024-03-31'
group by equipment_id;

-- Q8. 문제 보고가 있는 기록에 등장한 장비 ID 목록을 조회하시오.
select equipment_id
from usagelog
where issue_report is not null
group by equipment_id;

-- Q9. 2024-03-10 이후 사용된 사용자 ID 목록을 조회하시오.
select user_id
from usagelog
where use_date >= '2024-03-12'
group by user_id;

-- Q10. 문제가 보고된 기록에 등장한 사용자 ID 목록을 조회하시오.
select user_id
from usagelog
where issue_report is not null
group by user_id;

-- =========================================
-- [3] GROUP BY + 집계함수
-- =========================================

-- Q11. 장비별 사용 횟수를 조회하시오.
select equipment_id, count(*)
from usagelog
group by equipment_id;

-- Q12. 사용자별 사용 횟수를 조회하시오.
select user_id, count(*)
from usagelog
group by user_id;

-- Q13. 장비별 문제 보고 건수를 조회하시오.
select equipment_id, count(issue_report)
from usagelog
group by equipment_id;

-- Q14. 사용자별 가장 최근 사용일을 조회하시오.
select user_id, max(use_date)
from usagelog
group by user_id;

-- Q15. 장비별 최초 사용일을 조회하시오.
select equipment_id, min(use_date)
from usagelog
group by equipment_id;

-- =========================================
-- [4] GROUP BY + HAVING
-- =========================================

-- Q16. 사용 횟수가 2회 이상인 장비를 조회하시오.
select equipment_id, count(*)
from usagelog
group by equipment_id
having count(*) >= 2;

-- Q17. 사용 횟수가 2회 이상인 사용자를 조회하시오.
select user_id, count(*)
from usagelog
group by user_id
having count(*) >= 2;

-- Q18. 문제 보고가 1건 이상 있는 장비를 조회하시오.
select equipment_id, count(issue_report)
from usagelog
group by equipment_id
having count(issue_report) >= 1;

-- Q19. 가장 최근 사용일이 2024-03-12 이후인 사용자를 조회하시오.
select user_id, max(use_date)
from usagelog
group by user_id
having max(use_date) >= 2024-03-12;

-- Q20. 최초 사용일이 2024-03-05 이후인 장비를 조회하시오.
select equipment_id, min(use_date)
from usagelog
group by equipment_id
having min(use_date) >= '2024-03-05';

-- =========================================
-- [5] GROUP BY + WHERE + HAVING
-- =========================================

-- Q21. 2024-03-05 이후 기록만 대상으로, 사용 횟수가 2회 이상인 장비를 조회하시오.
select equipment_id, count(*)
from usagelog
where use_date >= '2024-03-05'
group by equipment_id
having count(*) >= 2;

-- Q22. 문제 보고가 있는 기록만 대상으로, 2건 이상 발생한 장비를 조회하시오.
select equipment_id, count(*)
from usagelog
where issue_report is not null
group by equipment_id
having count(*) >= 2;

-- Q23. 2024-03-10 이후 기록만 대상으로, 사용 횟수가 2회 이상인 사용자를 조회하시오.
select user_id, count(*)
from usagelog
where use_date >= '2024-03-10'
group by user_id
having count(*) >= 2;

-- Q24. 문제 없는 기록만 대상으로, 2회 이상 사용된 장비를 조회하시오.
select equipment_id, count(*)
from usagelog
where issue_report is null
group by equipment_id
having count(*) >= 2;

-- Q25. 2024-03-01 ~ 2024-03-12 기록 중, 최근 사용일이 2024-03-10 이후인 사용자를 조회하시오.
select user_id, max(use_date)
from usagelog
where use_date between '2024-03-01' and '2024-03-12'
group by user_id
having max(use_date) >= '2024-03-10';