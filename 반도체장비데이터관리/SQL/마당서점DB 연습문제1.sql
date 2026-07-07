# 연습문제 1.마당서점DB

use madangDB;
#1
select bookname, price as '도서 가격' from book;

#2
select name as '고객명', address as '거주지' from customer;

#3
select orderid, saleprice as '판매금액' from Orders;

# 집계함수

#1
select count(*) as '총 권수' from book;

#2
select count(*) as '전체 주문 건수' from orders;

#3
select count(*) as '전체 고객 수' from customer;

#4
select max(price) from book;

#5
select min(price) from book;

#6
select avg(price) as '도서 가격 평균' from book;

#7
select sum(saleprice) as '전체 판매 금액' from orders;

#8
select avg(saleprice) as '평균 판매 금액'  from orders;

#9
select min(orderdate) as '가장 빠른 주문일' from orders;

#10
select max(orderdate) as '가장 최근 주문일' from orders;

# 집계함수: 조건 포함 문제

#1
select count(*) from book where price >= 20000;

#2
select avg(price) from book where price < 15000;

#3
select count(*) from book where publisher = '굿스포츠';

#4
select max(price) as '최고가격' from book where publisher = '대한미디어';

#5
select count(*) as '주문 수' from orders;

#6
select sum(saleprice) as '총 판매액' from orders where saleprice >= '20000';

#7
select count(*) as '고객수' from customer where address like '대한민국%'; 

#8
select count(*) as '전화번호가 입력된 고객 수' from customer where phone is not null;

#9
select count(*) as '전화번호가 입력되지 않은 고객 수' from customer where phone is null;

#10
select count(distinct custid) as '주문고객 수' from orders;
