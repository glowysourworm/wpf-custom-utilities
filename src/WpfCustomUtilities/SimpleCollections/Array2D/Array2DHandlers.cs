namespace WpfCustomUtilities.SimpleCollections.Array2D
{
    /// <summary>
    /// Value to provide to iterator to signal when to terminate a loop
    /// </summary>
    public enum Array2DCallback
    {
        Continue = 0,
        BreakAndReturn = 1
    }

    /// <summary>
    /// Delegate for creating an iteration loop over the region boundary coordinate space
    /// </summary>
    public delegate Array2DCallback GridBoundaryIterator(int column, int row);

    /// <summary>
    /// Callback used for grid processing - provides the column, and row.
    /// </summary>
    public delegate Array2DCallback GridCallback(int column, int row);

    /// <summary>
    /// Callback used to provide a result at each index to the caller
    /// </summary>
    public delegate void GridProvideResult<T>(int column, int row, T result);

    /// <summary>
    /// Callback used to provide a result at each index
    /// </summary>
    public delegate T GridResult<T>(int column, int row);

    /// <summary>
    /// Callback used to provide a selector at each index that differs in type
    /// </summary>
    public delegate TResult GridSelector<T, TResult>(int column, int row, T item);

    /// <summary>
    /// Predicate that carries grid indicies along with the item. Used to make decisions during
    /// iteration
    /// </summary>
    public delegate bool GridPredicate<T>(int column, int row, T item);

    /// <summary>
    /// Predicate that carries grid indicies. Used to make decisions during iteration
    /// </summary>
    public delegate bool GridPredicate(int column, int row);
}
