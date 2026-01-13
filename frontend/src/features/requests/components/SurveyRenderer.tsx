import { useMemo, useCallback, useRef } from 'react';
import { Model } from 'survey-core';
import { Survey } from 'survey-react-ui';
import 'survey-core/survey-core.min.css';
import { auroraGlassTheme } from '../surveyjs-theme';
import '../surveyjs-overrides.css';
import { api } from '@/lib/api';

interface SurveyRendererProps {
  surveyJson: string;
  onComplete: (data: Record<string, unknown>, fileIds: string[]) => void;
  onPartialSave?: (data: Record<string, unknown>) => void;
  initialData?: Record<string, unknown>;
}

export function SurveyRenderer({
  surveyJson,
  onComplete,
  onPartialSave,
  initialData,
}: SurveyRendererProps) {
  // Track uploaded file IDs for linking after response submission
  const uploadedFileIds = useRef<string[]>([]);

  const survey = useMemo(() => {
    try {
      const model = new Model(JSON.parse(surveyJson));
      model.applyTheme(auroraGlassTheme);

      // Configure file upload handling
      model.onUploadFiles.add(async (_sender, options) => {
        const files = options.files;
        const questionName = options.question.name;

        try {
          const uploadPromises = files.map(async (file: File) => {
            const result = await api.uploadFile(file, questionName);
            uploadedFileIds.current.push(result.id);
            return {
              file: file,
              content: result.downloadUrl,
              // Store file ID in the content for later retrieval
              name: file.name,
              type: file.type,
            };
          });

          const results = await Promise.all(uploadPromises);
          options.callback(
            'success',
            results.map((r) => ({
              file: r.file,
              content: r.content,
            }))
          );
        } catch (error) {
          console.error('File upload failed:', error);
          options.callback('error');
        }
      });

      // Handle file removal
      model.onClearFiles.add(async (_sender, options) => {
        const value = options.value;

        if (Array.isArray(value)) {
          // Extract file IDs from the download URLs and delete them
          for (const fileData of value) {
            if (fileData.content && typeof fileData.content === 'string') {
              // Extract file ID from URL like "/api/files/{fileId}"
              const match = fileData.content.match(/\/api\/files\/([a-f0-9-]+)/i);
              if (match) {
                const fileId = match[1];
                try {
                  await api.deleteFile(fileId);
                  uploadedFileIds.current = uploadedFileIds.current.filter(
                    (id) => id !== fileId
                  );
                } catch (error) {
                  console.error('Failed to delete file:', error);
                }
              }
            }
          }
        }
        options.callback('success');
      });

      if (initialData) {
        model.data = initialData;
      }

      return model;
    } catch (error) {
      console.error('Failed to parse survey JSON:', error);
      return null;
    }
  }, [surveyJson, initialData]);

  const handleComplete = useCallback(
    (sender: Model) => {
      onComplete(sender.data, [...uploadedFileIds.current]);
      // Clear the file IDs after submission
      uploadedFileIds.current = [];
    },
    [onComplete]
  );

  const handleValueChanged = useCallback(
    (sender: Model) => {
      if (onPartialSave) {
        onPartialSave(sender.data);
      }
    },
    [onPartialSave]
  );

  if (!survey) {
    return (
      <div className="glass rounded-xl p-8 text-center">
        <p className="text-muted-foreground">Failed to load form</p>
      </div>
    );
  }

  survey.onComplete.add(handleComplete);

  if (onPartialSave) {
    survey.onValueChanged.add(handleValueChanged);
  }

  return (
    <div className="survey-container">
      <Survey model={survey} />
    </div>
  );
}

export default SurveyRenderer;
