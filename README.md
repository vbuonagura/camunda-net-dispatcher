# Introduction 
Fully working .NET 5 project that shows how to interact with Camunda to automatically execute Service Tasks and Receive Tasks
# Build and Test
Steps needed to run and test the project:
1.	Installation Visual Studio with the latest version of .NET
2.	Install Camunda (camunda-bpm-run version is enough but every version works)
3.	Add the proper value to the appsettings.json to connect to Camunda
4.	Create and deploy a BPMN process to Camunda with at least one service task with an external implementation and topic configured
5.  Create an executor class in the SampleWebApi project to execute some test code