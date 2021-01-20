using System;

namespace UpdateNight.TocReader.Parsers.Objects
{
    /** Allows area weighted sampling of triangles on a skeletal mesh. */
    public readonly struct FSkeletalMeshAreaWeightedTriangleSampler : IUStruct
    {
        /*
        public readonly USkeletalMesh Owner;
        public readonly int[] TriangleIndices;
        public readonly int LODIndex;
        */

        internal FSkeletalMeshAreaWeightedTriangleSampler(PackageReader reader)
        {
            throw new NotImplementedException(string.Format("Parsing of {0} types isn't supported yet.", "FSkeletalMeshAreaWeightedTriangleSampler"));
        }
    }
}