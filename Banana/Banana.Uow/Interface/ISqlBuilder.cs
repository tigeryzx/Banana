/***********************************
 * Developer: Lio.Huang
 * Date：2018-12-17
 * 
 * Last Update：2018-12-18
 **********************************/

namespace Banana.Uow.Interface
{
    /// <summary>
    /// The interface for all SqlBuilder operations.
    /// </summary>
    public interface ISqlBuilder
    {
        /// <summary>
        /// SQL
        /// </summary>
        string SQL { get; }

        /// <summary>
        /// 与SQL性质一样，为处理了转义后的SQL
        /// </summary>
        string ESQL { get; }

        /// <summary>
        /// args
        /// </summary>
        object Arguments { get; }
    }
}
