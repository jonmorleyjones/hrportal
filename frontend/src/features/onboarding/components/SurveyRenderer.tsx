import { useMemo, useCallback } from 'react';
import { Model } from 'survey-core';
import { Survey } from 'survey-react-ui';
import 'survey-core/survey-core.min.css';
import { auroraGlassTheme } from '../surveyjs-theme';
import '../surveyjs-overrides.css';

interface SurveyRendererProps {
  surveyJson: string;
  onComplete: (data: Record<string, unknown>) => void;
  onPartialSave?: (data: Record<string, unknown>) => void;
  initialData?: Record<string, unknown>;
}

export function SurveyRenderer({
  surveyJson,
  onComplete,
  onPartialSave,
  initialData,
}: SurveyRendererProps) {
  const survey = useMemo(() => {
    try {
      const model = new Model(JSON.parse(surveyJson));
      model.applyTheme(auroraGlassTheme);

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
      onComplete(sender.data);
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
        <p className="text-muted-foreground">Failed to load survey</p>
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
