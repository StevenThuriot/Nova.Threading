Nova Threading
====

* Action Queuing system designed for the Nova project.
	* Branched into this repository after growing enough to be a standalone project.
* Allows queuing through a manager on several queues.
* A queue can be created, destroyed or blocked.
* A blocked queue will not execute anything.
* Queues are built on top of .NET's dataflow library.
* Actions that belong to non-existant queues don't get executed.
* However, they can be marked to run unqueued.
* Metadata is easy to configure using attributes. (e.g. Blocking, Creational, ...)
* Action implementation is decoupled from the queuing system. A WPF specific dll has already been made.
* Available on NuGet.org and SymbolSource.org as
	* Nova.Threading
	* Nova.Threading.WPF
	* Nova.Threading.Metadata
