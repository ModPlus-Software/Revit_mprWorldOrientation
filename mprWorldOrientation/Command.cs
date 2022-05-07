namespace mprWorldOrientation;

using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModPlus_Revit;
using ModPlusAPI.Windows;
using ModPlusStyle.Controls;
using mprWorldOrientation.ViewModels;
using mprWorldOrientation.Views;

/// <inheritdoc />
[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
public class Command : IExternalCommand
{
    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
#if !DEBUG
            ModPlusAPI.Statistic.SendCommandStarting(new ModPlusConnector());
#endif
            ModPlus.ShowModal(new MainWindow(new MainContext()));
            return Result.Succeeded;
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return Result.Cancelled;
        }
        catch (Exception exception)
        {
            exception.ShowInExceptionBox();
            return Result.Failed;
        }
    }
}