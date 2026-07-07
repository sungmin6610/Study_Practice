# 가격이 8000원 이상인 도서를 구매한 고객/ 
# 고객별 주문 도서의 총 수량 /
# 단, 2권 이상 구매한 고객만 출력 /
use madangDB;
select custid
from orders
where saleprice >= 8000;

select custid, count(*)
from orders
where saleprice >= 8000
group by custid
having count(*) >= 2;

-- 반도체장비데이터관리 03강 
-- GROUP BY + HAVING 연습문제

-- 기본 문제
-- 1. 주문 횟수가 2회 이상인 고객을 조회하시오.
select custid, count(*)
from orders
group by custid
having count(*) >= 2;

-- 2. 총 구매 금액이 30,000원 이상인 고객을 조회하시오.
select custid, sum(saleprice)
from orders
group by custid
having sum(saleprice) >= 30000;

-- 3. 평균 구매 금액이 15,000원 이상인 고객을 조회하시오.
select custid, avg(saleprice)
from orders
group by custid
having avg(saleprice) >= 15000;

-- 4. 판매 횟수가 2회 이상인 도서를 조회하시오.
select bookid, count(*)
from orders
group by bookid
having count(*) >= 2;

-- 5. 최소 구매 금액이 10,000원 이상인 고객을 조회하시오.
select custid, min(saleprice)
from orders
group by custid
having min(saleprice) >= 10000;

-- 조건 결합 문제
-- 1. 판매가격이 10,000원 이상인 주문만 고려하여, 주문 횟수가 2회 이상인 고객을 조회하시오.
-- 2. 2024년 이후 주문만 고려하여, 총 구매 금액이 20,000원 이상인 고객을 조회하시오.
-- 3. 도서별 총 판매 금액이 30,000원 이상인 도서를 조회하시오.
-- 4. 고객별 최고 구매 금액이 20,000원 이상인 고객을 조회하시오.
-- 5. 고객별 평균 구매 금액이 12,000원 이상인 고객을 조회하시오.