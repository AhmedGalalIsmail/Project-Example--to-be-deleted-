Imports System.Windows.Forms
Imports Domain.Entities
Imports Domain.Services

''' <summary>
''' LayerPanel is a UserControl that provides a UI for managing layers in the application. It allows users to view existing layers, add new layers, and delete selected layers. The control interacts with a LayerManager to perform these operations and updates its display accordingly.</summary>
Public Class LayerPanel
	Inherits UserControl

    ' ? CONTROL NAMES (as you requested)
    ''' <summary>
    ''' ListBox to display existing layers. Shows layer names and allows selection for deletion.
    ''' </summary>
    Private lstLayers As New ListBox()
	Private btnAddLayer As New Button()
	Private btnDeleteLayer As New Button()

	Private _layerManager As LayerManager

	''' <summary>
	''' Initializes LayerPanel with LayerManager dependency.
	''' </summary>
	Public Sub Initialize(manager As LayerManager)
		_layerManager = manager
		RefreshLayers()
	End Sub

    ''' <summary>
    ''' Constructor sets up UI controls and layout. Initializes default size and docking.
    ''' </summary>
    Public Sub New()
		Me.Width = 200
		Me.Dock = DockStyle.Left
		SetupControls()
	End Sub

    ''' <summary>
    ''' Configures UI controls: positions ListBox at the top, buttons below it. Sets button text and event handlers.
    ''' </summary>
    Private Sub SetupControls()
		lstLayers.Dock = DockStyle.Top
		lstLayers.Height = 200

		btnAddLayer.Text = "Add Layer"
		btnDeleteLayer.Text = "Delete Layer"

		btnAddLayer.Dock = DockStyle.Top
		btnDeleteLayer.Dock = DockStyle.Top

		AddHandler btnAddLayer.Click, AddressOf AddLayer_Click
		AddHandler btnDeleteLayer.Click, AddressOf DeleteLayer_Click

		Me.Controls.Add(btnDeleteLayer)
		Me.Controls.Add(btnAddLayer)
		Me.Controls.Add(lstLayers)

	End Sub

	''' <summary>
	''' Refreshes the ListBox to reflect the current state of layers 
	''' in LayerManager. Clears existing items and repopulates 
	''' with current layer names.
	''' </summary>
	Private Sub RefreshLayers()
		lstLayers.Items.Clear()
		For Each layer In _layerManager.GetAll()
			lstLayers.Items.Add(layer.Name)
		Next
	End Sub

    ''' <summary>
    ''' Event handler for Add Layer button. Creates a new layer with a default name 
    ''' based on the current count of layers, adds it to LayerManager, and refreshes the list.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AddLayer_Click(sender As Object, e As EventArgs)
		Dim name = "Layer_" & (_layerManager.GetAll().Count + 1)
		_layerManager.AddLayer(name)
		RefreshLayers()
	End Sub

	''' <summary>
	''' Event handler for Delete Layer button. Checks if a layer is selected in the ListBox, 
	''' retrieves the corresponding layer from LayerManager, removes it, and refreshes the list.
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	Private Sub DeleteLayer_Click(sender As Object, e As EventArgs)
		If lstLayers.SelectedIndex < 0 Then Exit Sub
		Dim selectedLayer = _layerManager.GetAll()(lstLayers.SelectedIndex)
		_layerManager.RemoveLayer(selectedLayer.Id)
		RefreshLayers()
	End Sub

End Class
