# how to run tests

In order to run the tests you have to execute the file runTest.js with node.

If you want to run tests in all available tenants in config.json you should run the following command:

node runTests.js -t all

If you want to run test in a specific tenant you should run the command like this:

node runTests.js -t [tenantName]

You have a config.json file which indicates the tenant where the tests are going to be ran.
In order to configurate a tenant you have to provide tenant name, domain,tenantAppDomain, adminUserEmail and private key.

You will probably need to get the dependecies, in order to do that, run the command "npm install" in a console.

Tests:
- create service on cloud
- create service on agent
- create DS
- create an App
- invoke a service on cloud
- invoke a service on agent
- invoke a service from the apphost
- invoke a DS
- invoke a DS from apphost
- create a storeage item
- get storage item from mkt
- get storage item from apphost
- delete storage item
- delete DS
- delete service on cloud
- delete service on agent
- delete app

## how to execute ios app test

In order to execute the automated ios test, which open an app with a visualization, you have to execute the file visualizationAut.command

- You need to have a application already compiled
- You need to edit the file visualizationAut.command, and set you own .app location, in my case is:

~/Library/Developer/Xcode/DerivedData/DataVizSampleApp-beoeeqmzlhjjttcijqyiufptxbxi/Build/Products/Release-iphonesimulator/DataVizSampleApp.app