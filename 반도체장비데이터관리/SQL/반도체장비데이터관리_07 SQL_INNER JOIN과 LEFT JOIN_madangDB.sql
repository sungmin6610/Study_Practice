-- 문제 1
-- 고객 이름과 주문번호를 조회하세요.
-- 주문이 있는 고객만 조회되도록 작성하세요.
use madangdb;
select c.name, o.orderid
from customer as c
join orders as o on c.custid = o.custid;

-- 문제 2
-- 모든 고객의 이름과 주문번호를 조회하세요.
-- 주문 이력이 없는 고객도 함께 나타나도록 작성하세요.
select *
from customer as c
left join orders as o on c.custid = o.custid;

-- 문제 3
-- 문제 1과 문제 2의 결과를 비교하여,
-- 어떤 고객이 LEFT JOIN에서만 나타나는지 확인하세요.
-- NULL 값이 있는 고객도 LEFT JOIN에서 나타난다.


-- [고객 기준 LEFT JOIN]

-- 문제 4
-- 모든 고객의 고객번호, 이름, 주문번호, 주문일자를 조회하세요.
-- 주문이 없는 고객도 포함하세요.
select c.custid, c.name, o.orderid, o.orderdate
from customer as c
left join orders as o 
on c.custid = o.custid;

-- 문제 5
-- 주문 이력이 없는 고객의 고객번호와 이름만 조회하세요.
select c.custid, c.name
from customer as c
left join orders as o 
on c.custid = o.custid 
where o.orderid is null;

-- [도서 기준 INNER JOIN / LEFT JOIN 비교]

-- 문제 6
-- 도서명과 주문번호를 조회하세요.
-- 주문된 도서만 조회되도록 작성하세요.
select b.bookname, o.orderid
from book as b
inner join orders as o 
on b.bookid = o.bookid
where o.orderid is not null;

-- 문제 7
-- 모든 도서의 도서번호, 도서명, 주문번호를 조회하세요.
-- 판매된 적 없는 도서도 함께 나타나도록 작성하세요.
select b.bookid, b.bookname, o.orderid
from book as b
left join orders as o 
on b.bookid = o.bookid
where o.orderid is null;

-- 문제 8
-- 한 번도 판매되지 않은 도서의 도서번호와 도서명만 조회하세요.
select b.bookid, b.bookname
from orders as o
left join book as b 
on b.bookid = o.bookid
where o.bookid is null;


-- [LEFT JOIN + 정렬]

-- 문제 9
-- 모든 고객의 이름과 주문번호를 조회하되,
-- 고객번호 오름차순으로 정렬하세요.
select c.name, o.orderid
from customer as c
left join orders as o 
on c.custid = o.custid
order by c.custid asc;


-- 문제 10
-- 모든 도서의 도서명과 주문번호를 조회하되,
-- 도서번호 오름차순으로 정렬하세요.
select b.bookname, o.orderid
from book as b
left join orders as o 
on b.bookid = o.bookid
order by b.bookid asc;