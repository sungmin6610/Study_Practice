use madangdb;
show tables;


#5
select * 
from Book 
where price >= 20000;

#6
select * 
from Customer 
where address = '대한민국 서울';

#7
select * 
from Orders
where saleprice >= 15000;

#8
select * 
from Book 
where publisher = "굿스포츠";

#9
select * 
from Book 
where price <= 10000;

#10
select *
from Orders 
where saleprice >= 20000;

#11
select * 
from Book 
where price <> 30000;

#12
select * 
from Book 
where price between 10000 and 20000;

#13
select * 
from Orders 
where saleprice between 15000 and 25000;

#14
select * 
from Book 
where publisher in ('굿스포츠', '대한미디어');

#15
select * 
from Customer 
where address in ('대한민국 서울', '대한민국 경기도');

#16
select * 
from Book 
where publisher not in ('굿스포츠');

#17
select * 
from Customer 
where address like '%대한민국 서울%';

#18
select * 
from Book 
where bookname like '%축구%';

#19
select * 
from Book 
where bookname like '%야구%';

#20
select * 
from Book 
where publisher = '굿스포츠' and price >= 20000;

#21
select * 
from Orders 
where saleprice >= 20000 and custid = '2';

#22
select * 
from Customer 
where (address like '서울%') or (phone IS NULL);

#23
select * 
from Book 
order by price asc;

#24
select * 
from Book 
order by price desc
limit 1;

#25
select * 
from Orders 
order by saleprice desc;

#26
select * 
from Customer 
order by name asc;

#27
select distinct publisher
from Book;


#28
select distinct custid
from Orders;

#29
select distinct price
from Book;