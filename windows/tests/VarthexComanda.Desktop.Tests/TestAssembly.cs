// WPF shares one Application and Dispatcher. ShowDialog pumps nested frames, so
// concurrent test collections can enter each other's modals and leak focus.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
