Imports Domain.Entities

Namespace AI
	Public Class AiIntakeService
#Region "ReadOnly Fields"
		''' <summary>
		''' The AiIntakeService is responsible for processing architectural drawing files, extracting relevant information such as text annotations, scale, and geometry. It uses various helper services to perform OCR, detect scales, and identify geometric elements in the drawings. The result is encapsulated in an AiIntakeResult object that contains the detected text, scale, and any identified elements from the drawing.</summary>
		Private ReadOnly _loader As New DrawingLoader()
		''' <summary>
		''' The OcrService is responsible for performing Optical Character Recognition on the drawing images to extract text information. In this offline-friendly implementation, it returns a deterministic set of text samples that simulate the output of an OCR engine, allowing the rest of the AI pipeline to function without requiring native OCR packages.</summary>
		Private ReadOnly _ocr As New OcrService()
		''' <summary>
		''' The ScaleDetector is responsible for analyzing the extracted text from the drawing and identifying any scale information present. It looks for common scale annotations (e.g., "SCALE 1:100") and extracts the relevant scale factor, which can be used for accurate measurements and takeoff calculations in the construction process.</summary>
		Private ReadOnly _scaleDetector As New ScaleDetector()
		''' <summary>
		''' The GeometryDetector is responsible for analyzing the extracted text and identifying basic geometric shapes and dimensions mentioned in the drawing. It uses pattern recognition to detect common geometric annotations (e.g., "5.0 x 4.0" for a rectangle) and creates CanvasElement objects with geometry information that can be used for further processing in the takeoff calculations.</summary>
		Private ReadOnly _geometry As New GeometryDetector()
		''' <summary>
		''' The ShapeDetector is responsible for analyzing the drawing (image) and detecting shapes, dimensions, and geometry information. In a real implementation, it would use image processing techniques to identify geometric elements in the drawing, but in this offline-friendly version, it returns an empty list of elements to allow the AI pipeline to function without native image processing packages.</summary>
		Private ReadOnly _shape As New ShapeDetector()
		''' <summary>
		''' The CategoryClassifier is responsible for classifying detected elements and text into predefined categories (e.g., walls, doors, windows) based on their characteristics. This classification can help in organizing the elements for further processing, such as building layers and applying materials in the takeoff calculations. In this implementation, it serves as a placeholder for a more complex classification logic that would be developed in future iterations of the AI pipeline.</summary>
		Private ReadOnly _scale As New ScaleDetector()
		''' <summary>
		''' The CategoryClassifier is responsible for classifying detected elements and text into predefined categories (e.g., walls, doors, windows) based on their characteristics. This classification can help in organizing the elements for further processing, such as building layers and applying materials in the takeoff calculations. In this implementation, it serves as a placeholder for a more complex classification logic that would be developed in future iterations of the AI pipeline.</summary>
		Private ReadOnly _classifier As New CategoryClassifier()
		''' <summary>
		''' The LayerAutoBuilder is responsible for automatically building layers of elements based on their classifications and relationships. It takes the classified elements and organizes them into layers that can be used for visualization and further processing in the takeoff calculations. In this implementation, it serves as a placeholder for a more complex layer-building logic that would be developed in future iterations of the AI pipeline.</summary>
		Private ReadOnly _layerBuilder As New LayerAutoBuilder()
		''' <summary>
		''' The MaterialMapper is responsible for mapping detected elements and their classifications to specific materials or construction components. It uses the classification results to determine the appropriate materials for each element, which can be used in the takeoff calculations to estimate quantities and costs. In this implementation, it serves as a placeholder for a more complex material mapping logic that would be developed in future iterations of the AI pipeline.</summary>
		Private ReadOnly _mapper As New MaterialMapper()
#End Region

#Region "Public Methods"
		''' <summary>
		''' Processes a drawing file, extracting text and detecting scale.</summary>
		''' <param name="filePath"></param>
		''' <returns>An AiIntakeResult containing the detected text, scale, and geometry elements from the drawing. In a real implementation, this would involve OCR processing and analysis of the drawing content.</returns>
		Public Function ProcessDrawing(filePath As String) As AiIntakeResult
			Dim image = _loader.LoadImage(filePath)
			Dim text = _ocr.ExtractText(image)
			Dim scale = _scaleDetector.DetectScale(text)
			Dim elements = _geometry.Detect(text) ' ? NEW

			Return New AiIntakeResult With {
			.DetectedText = text,
			.DetectedScale = scale,
			.DetectedElements = elements
		}
		End Function
#End Region

		'-------------------------------------------
		'Public Function ProcessDrawing(filePath As String) As AiIntakeResult
		'    Dim image = _loader.LoadImage(filePath)
		'    Dim text = _ocr.ExtractText(image)
		'    Dim scale = _scaleDetector.DetectScale(text)
		'    Return New AiIntakeResult With {
		'        .DetectedText = text,
		'        .DetectedScale = scale
		'    }
		'End Function

		'-------------------------------------------
		'Public Function ProcessDrawing(filePath As String) As AiIntakeResult
		'	'Dim elements = _shape.DetectShapes(filePath)
		'	' ? Real OCR
		'	Dim text = _ocr.ExtractText(filePath)

		'	' ? Real Scale Detection
		'	Dim scale = _scale.DetectScale(text)

		'	' ? Real Shape Detection
		'	Dim elements = _shape.DetectShapes(filePath)
		'	Dim layout As New CanvasLayout()
		'	layout.Elements.AddRange(elements)
		'	' ? 1. CLASSIFY
		'	Dim classification = _classifier.Classify(elements, text)
		'	' ? 2. BUILD LAYERS
		'	Dim layers = _layerBuilder.BuildLayers(layout, classification)
		'	' ? 3. APPLY MATERIALS
		'	_mapper.Apply(elements, classification)
		'	Return New AiIntakeResult With {
		'		.DetectedText = text,
		'		.DetectedScale = scale,
		'		.DetectedElements = elements
		'	}

		'End Function

	End Class
End Namespace
