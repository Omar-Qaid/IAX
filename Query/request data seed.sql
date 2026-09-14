

SELECT  [RequestId]
      ,[EmployeeId]
      ,[ProcessId]
      ,[RequestDate]
      ,[RequestDetails]
      ,[IsFinished]
      ,[Hidden]
      ,[CreatedDate]
      ,[CreatedBy]
      ,[AttachmentId]
      ,[IsPin]
      ,[PinDate]
      ,[Priority]
      ,[IsStopped]
      ,[Notes]
      ,[FinishedDate]
      ,[StoppedDate]
      ,[Points]
      ,[Degree]
      ,[TargetWeight]
      ,[ActualWeight]
      ,[RequestType]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfRequests]  where [ProcessId]=590 and  RequestId=127677   order by   [RequestId] desc

SELECT TOP (1000) [RequestDetailId]
      ,[RequestId]
      ,[ControlId]
      ,[ControlDataId]
      ,[ControlLabel]
      ,[ControlLabelAR]
      ,[ControlValue]
      ,[UsedAsCriteria]
      ,[ControlOrder]
      ,[RelatedObjectId]
      ,[ControlValueAR]
      ,[ControlValueEN]
      ,[TargetWeight]
      ,[ActualWeight]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfRequestDetails] where  RequestId=127677  

  SELECT TOP (1000) [RequestId]
      ,[VariableId]
      ,[VariableValue]
      ,[VariableOrder]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfRequestVariables] where RequestId=   127677

  SELECT TOP (1000) [ProcessVariableId]
      ,[RequestId]
      ,[VariableId]
      ,[VariableValue]
      ,[VariableOrder]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfProcessVariables] where  RequestId=127677



SELECT TOP (1000) [AssignmentID]
      ,[RequestID]
      ,[ActivityID]
      ,[UserID]
      ,[AssignDate]
      ,[Finished]
      ,[AutoPassing]
      ,[PeriodHrs]
      ,[StepId]
      ,[Automatically]
      ,[FinishedDate]
      ,[Transferred]
      ,[ActualWeight]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfAssignments] where  RequestId=127677


SELECT TOP (1000) [TaskID]
      ,[AssignmentID]
      ,[FinishDate]
      ,[ActivityDetails]
      ,[ExtendedProperties]
      ,[AttachmentId]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfProcessData] where [AssignmentID] in ( select   [AssignmentID]FROM [db_a8e163_aljazerasoftfp].[dbo].[WfAssignments] where  RequestId=127677)


SELECT TOP (1000) [ActivityDetailID]
      ,[TaskID]
      ,[AssignmentID]
      ,[ControlId]
      ,[ControlDataId]
      ,[ControlLabel]
      ,[ControlLabelAR]
      ,[ControlValue]
      ,[UsedAsCriteria]
      ,[ControlOrder]
      ,[RelatedObjectId]
      ,[ControlValueAR]
      ,[ControlValueEN]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfActivityDetails] where [AssignmentID] in ( select   [AssignmentID]FROM [db_a8e163_aljazerasoftfp].[dbo].[WfAssignments] where  RequestId=127677)




  SELECT TOP (1000) [MappingId]
      ,[ActivityControlID]
      ,[VariableID]
      ,[Activated]
      ,[VariableOrder]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfActivityMappingVariables]


    where  [VariableId] in (SELECT [VariableId]
    
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfVariables] where ProcessId =603)



  SELECT TOP (1000) [MappingId]
      ,[RequestControlID]
      ,[VariableID]
      ,[Activated]
      ,[VariableOrder]
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfRequestMappingVariables]

  where  [VariableId] in (SELECT [VariableId]
    
  FROM [db_a8e163_aljazerasoftfp].[dbo].[WfVariables] where ProcessId =603)