namespace DontBeLazy.Ports.DTOs;

// Enum mirrors — UI chỉ biết các enum này, không dùng Domain.Enums
public enum TaskStatusDto     { Pending, Active, Done, Abandoned }
public enum ProfileEntryTypeDto { Website, App }
public enum CompletionStatusDto { Completed, CompletedEarly, StillWorking, Abandoned }
public enum QuoteEventTypeDto   { PreFocus, MidFocus, PostFocus, GiveUp, Random }
