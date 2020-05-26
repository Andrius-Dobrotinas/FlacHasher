using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public class CompressionLevelService
    {
        private readonly CompressionLevelInferrer metadataPreservedService;
        private readonly CompressionLevelInferrer metadataDiscardedService;

        public CompressionLevelService(
            CompressionLevelInferrer metadataPreservedService,
            CompressionLevelInferrer metadataDiscardedService)
        {
            this.metadataPreservedService = metadataPreservedService;
            this.metadataDiscardedService = metadataDiscardedService;
        }

        public Range<uint> InferCompressionLevel(FileInfo sourceFile, uint targetCompressionLevel, MetadataMode metadataMode)
        {
            CompressionLevelInferrer service = PickEncoder(metadataMode);

            return service.InferCompressionLevel(sourceFile, targetCompressionLevel);
        }

        private CompressionLevelInferrer PickEncoder(MetadataMode metadataMode)
        {
            switch (metadataMode)
            {
                case MetadataMode.Preserve:
                    return metadataPreservedService;
                case MetadataMode.Discard:
                    return metadataDiscardedService;
                default: throw new NotSupportedException("The mode is supported");
            }
        }
    }
}