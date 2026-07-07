create table newbook(
bookname varchar(20) not null,
publisher varchar(20) unique,
price integer default 10000 check(price >= 1000),
primary key (bookname, publisher));

CREATE TABLE NewCustomer (
    custid   INTEGER PRIMARY KEY,
    name     VARCHAR(40),
    address  VARCHAR(40),
    phone    VARCHAR(30)
);
CREATE TABLE NewOrders (
    orderid    INTEGER,
    custid     INTEGER NOT NULL,
    bookid     INTEGER,
    saleprice  INTEGER,
    orderdate  DATE,
    PRIMARY KEY(orderid),
    FOREIGN KEY(custid)
        REFERENCES NewCustomer(custid)
        ON DELETE CASCADE
);

select * from newbook;
select * from NewCustomer;
select * from NewOrders;

INSERT INTO NewCustomer VALUES (1, 'Alice', 'Seoul', '010-1111-1111');
INSERT INTO NewCustomer VALUES (2, 'Bob', 'Busan', '010-2222-2222');
INSERT INTO NewBook VALUES ('SQL입문', '한빛출판사', 15000);
INSERT INTO NewBook (bookname, publisher)
VALUES ('AI개론', '위키북스');

INSERT INTO NewOrders VALUES (1, 1, 101, 12000, '2024-03-01');
INSERT INTO NewOrders VALUES (3, 1, 9999, 13000, '2024-03-03');

ALTER TABLE NewBook ADD bookid INTEGER;
UPDATE NewBook SET bookid = 1 WHERE bookname = 'SQL입문';
UPDATE NewBook SET bookid = 2 WHERE bookname = 'AI개론';

ALTER TABLE NewBook DROP PRIMARY KEY;
ALTER TABLE NewBook ADD PRIMARY KEY (bookid);

ALTER TABLE NewOrders
ADD CONSTRAINT fk_book
FOREIGN KEY (bookid)
REFERENCES NewBook(bookid);

delete from neworders
where bookid not in = 9999;