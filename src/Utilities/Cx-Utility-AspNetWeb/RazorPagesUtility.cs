using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CxUtility.AspNetWeb;

public static class RazorPagesUtility
{
    /*
     
     RazorPage<TModel>
     ViewDataDictionary<TModel>
    
    //public static void ViewData(this ViewDataDictionary<dynamic> ViewData) 

     */

    /// <summary>
    /// Sets the ViewData["Title"]
    /// </summary>
    /// <param name="razorPage">The Razor Page collection that the title is attched to. </param>
    /// <param name="Title">The title value to apply</param>
    public static void Set_Title(this RazorPage<dynamic> razorPage, string Title) => 
        razorPage.ViewData.Set_Title(Title);

    /// <summary>
    /// Sets the ViewData["Title"]
    /// </summary>
    /// <param name="ViewData">The ViewData Collect to Attach the Title value Too</param>
    /// <param name="Title">The title value to apply</param>
    public static void Set_Title(this ViewDataDictionary<dynamic> ViewData, string Title)
    {
        if (Title.hasNoCharacters())
            return;

       ViewData[nameof(Title)] = Title;
    }
}
