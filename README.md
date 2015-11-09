# DeveloperAssessment
Kpmg Developer Assessment

Below is a quick overview of things i managed to achieve and those which due to time constraints, i have decided to push down its priority level.

COMPLETED TASK
===============
- File upload 
- Validation of each line before persistence to the database
- Validation of ISO 4217 currency code before persistence to the database
- Feedback on the number of successful lines saved to the database
- Custome Multipart content formatter that wraps the request to any indicated model
- Dynamic assembly loading..ensures loose coupling in the application
- Comprehensive details of data rows that failed validation and subsequently didn't make it into the valid data table
- Users can see all the valid data transaction in the database, these can be filtered and sorted [i will talk a bit about these in the TODO section]
- Transaction data can be updated or deleted.
- Tried to implement SOLID and DRY principles reasonably within the constrains given.

TODOs
======
- Regarding displaying the list of all transaction data in the database, i have lazy loaded these. Ideally all data will not be loaded immediately, rather it should be loaded on demand. This approach ensure reliable performance and durability...especially as the records can be over 1m+

What i had set up in mind to do was each page to be loaded by the server and each page data chunk returned to the client, but due to time constraints i didn't get round to implementing this...although this will involve the client firing off multiple request for each page its gains compared to loading 1m+ records is unquestionable.

- Logging: Ideally i would have wanted to implement some logging in the code but i didn't round to doing this.
- Unit Tests: Personally this is the most important omission, as normally this helps massively during my development but due to constraints, i just had to be smart and prioritise what feature i was mostly interested in demonstrating.

SUMMARY
=======
So basically i have dedicated most of the time alloted to this task towards the architecture, specifically the backend as i strongly believe a solidly architected backend will ultimately drive the UI. Hence why i didn't really do much with the UI.

I had to make that decision in order to gain myself time to design the backend close enough to how i'd like it.

Thank you for the opportunity and all the best to me :)
