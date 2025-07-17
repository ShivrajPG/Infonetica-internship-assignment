The Infonetica Software Engineer Intern assignment :-

Hello, my self Shivraj Gulve, I am third year undergradute student at IIT Kharagpur, and this is my assignment solution for Infonetica Software Engineer Intern.

The objective was to create a simple backend that behaves similarly to a configurable state machine (workflow engine). It should allow you to define workflows, instantiate them, transition between states via actions, and validate each step along the way.

What This Project Does :- 

  - You can declare a workflow with states and actions.
  - You can initiate an instance of a workflow — it starts at the first state.
  - You can invoke actions to advance the instance to the next state.
  - Only valid transitions are permitted (according to rules specified).
  - All actions executed on an instance are remembered (with timestamps).
  - Data is retained in memory with thread-safe dictionaries.

  How to Run & Test:-
   - .NET 8 SDK installed
   - command - dotnet run
