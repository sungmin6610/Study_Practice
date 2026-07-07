-- =========================================
-- 마당서점 DB LEFT JOIN + GROUP BY 실습문제
-- 테이블
-- Book(bookid, bookname, publisher, price)
-- Orders(orderid, custid, bookid, saleprice, orderdate)
-- Customer(custid, name, address, phone)
-- =========================================

use madangdb;

-- 문제 1
-- 모든 고객의 고객번호, 이름, 주문 건수를 조회하세요.
-- 주문이 없는 고객도 포함하세요.
select c.custid, c.name, count(orderid)
from customer as c
left join orders as o 
on c.custid = o.custid
group by c.custid, c.name;

-- 문제 2
-- 모든 도서의 도서번호, 도서명, 판매 건수를 조회하세요.
-- 판매된 적 없는 도서도 포함하세요.
select b.bookid, b.bookname, count(orderid)
from book as b
left join orders as o 
on b.bookid = o.bookid
group by b.bookid, b.bookname;

-- 문제 3
-- 고객별 총 주문금액을 조회하세요.
-- 주문이 없는 고객도 포함하세요.
select c.custid, c.name ,sum(saleprice)
from customer as c
left join orders as o 
on c.custid = o.custid
group by c.custid, c.name;

-- 문제 4
-- 판매된 도서 수를 고객별로 조회하세요.
-- 주문이 없는 고객도 포함하세요.
select c.custid, c.name, count(orderid)
from orders as o
left join customer as c 
on o.custid = c.custid
group by o.custid;

-- 문제 5
-- 출판사별 판매 건수를 조회하세요.
-- 단, 판매되지 않은 도서도 결과에 반영되도록 작성하세요.
select b.publisher, count(o.orderid)
from book as b
left join orders as o 
on b.bookid = o.bookid
group by b.publisher;

-- 문제 6
-- 주문이 없는 고객만 조회하세요.
-- 고객번호, 이름, 주문 건수를 출력하세요.
select c.custid, c.name, count(o.orderid)
from customer as c
left join orders as o 
on c.custid = o.custid
group by c.custid, c.name
having count(o.orderid) = 0;

-- 문제 7
-- 한 번도 판매되지 않은 도서만 조회하세요.
-- 도서번호, 도서명, 판매 건수를 출력하세요.
select b.bookid, b.bookname, count(o.orderid)
from book as b
left join orders as o 
on b.bookid = o.bookid
where orderid is null;

-- 문제 8
-- 고객별 주문 건수를 조회하되,
-- 주문 건수가 많은 순서대로 정렬하세요.
select c.custid, c.name, count(o.orderid)
from customer c
left join orders as o
on c.custid = o.custid
group by c.custid, c.name
order by count(o.orderid) desc;

-- 문제 9
-- 도서별 판매 건수를 조회하되,
-- 판매 건수가 적은 순서대로 정렬하세요.
select b.bookid, b.bookname, count(o.orderid)
from book as b
left join orders as o
on b.bookid = o.bookid
group by b.bookid, b.bookname 
order by count(o.orderid) desc;

-- 문제 10
-- 주문이 1건 이상인 고객만 조회하세요.
-- 고객번호, 이름, 주문 건수를 출력하세요.