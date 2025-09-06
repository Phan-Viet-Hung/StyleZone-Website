namespace API.Domain.Validate.IExcelValidator
{
    public interface IExcelValidator<T>
    {
        /// <summary>
        /// Validate từng object import, trả về danh sách lỗi nếu có
        /// </summary>
        List<string> Validate(T entity);
    }

}
