using System;
using System.Collections.Generic;

public class TrialCourseLayout
{
    public IList<CourseChunk> ChunkList { get; }

    public TrialCourseLayout(IList<CourseChunk> chunkList)
    {
        ChunkList = chunkList;
    }
}
