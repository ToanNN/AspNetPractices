# EF Core
EF Core interprets a property as a foreign key if it's named <navigation property name><primary key property name>. For example,StudentID is the foreign key for the Student navigation property, since the Student entity's primary key is ID. Foreign key properties can also be named <primary key property name>. 
For example, CourseID since the Course entity's primary key is CourseID.