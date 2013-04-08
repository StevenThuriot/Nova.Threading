Nova Threading
====

* Action Queuing system designed for the Nova project.
* Allows queuing through a manager on several queues.
* A queue can be created, destroyed or blocked.
* A blocked queue will not execute anything.
* Queues are built on top of .NET's dataflow library.
* Actions that belong to non-existant queues don't get executed.
* Action implementation is decoupled from the queuing system. A WPF specific dll has already been made, based on the TPL.