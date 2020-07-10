drop table FOLDER;

create table FOLDER(
    ID BigInt PRIMARY KEY AUTO_INCREMENT,
    LONGDESCRIPTION varchar,
    COURSEID BigInt,
    CREATEDBY BigInt,
    ISPOSED BOOLEAN,
    ENDDATE TimeStamp,
    STARTDATE TimeStamp,
    FOREIGN KEY (COURSEID) REFERENCES COURSE(ID)
);

insert into FOLDER Values(
    1,
    'Homework 1',
    1,
    1,
    false,
    null,
    null
);

insert into FOLDER Values(
    2,
    'Homework 2',
    1,
    1,
    false
);

drop table PROBLEMTOFOLDER;

create table PROBLEMTOFOLDER(
    ID BigInt PRIMARY KEY AUTO_INCREMENT,
    PROBLEMID BigInt,
    FOLDERID BigInt,
    FOREIGN KEY (PROBLEMID) REFERENCES PROBLEM(ID),
    FOREIGN KEY (FOLDERID) REFERENCES FOLDER(ID)
);

insert into PROBLEMTOFOLDER Values(
    2, 
    1
);