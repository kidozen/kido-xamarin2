
var target = UIATarget.localTarget();

var app = target.frontMostApp().mainWindow();


target.delay(2);
app.textFields()[0].textFields()[0].setValue("WebinarTest");
target.delay(2);
app.buttons()["Show!"].tap();
target.delay(10);
app.scrollViews()[0].webViews()[0].textFields()["qty"].setValue(5);
target.delay(1);
app.scrollViews()[0].webViews()[0].buttons()["Refresh"].tap();
target.delay(10);