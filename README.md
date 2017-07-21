# SQLServer-selected-backup

Allows to do fast selective backup in a SQLServer catalog.

[image1]: ./images/SSSB.jpg


### Application description

This app is created to do backup of several tables in a database an restore the data in another with the same structure. It is useful for copying information from production to development environments avoiding tables with restricted information (like personal data). It can be used to reproduce production bugs in test or development paçlatforms.

### Application use

When you start the application a form with connection data is shown. After filling the information you should click the connection button. Leave user name and password blank to use windows authentication.

When connection has been done, you can see a list of tables of the database. You can select several tables and make a copy. You can also restore the last copy to the connected DB.

![alt text][image1]


### Contributing

Summary of guidelines for contributions:

* One pull request per issue;
* Choose the right base branch;
* Include tests and documentation;
