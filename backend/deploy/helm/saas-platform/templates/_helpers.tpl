{{- define "saas-platform.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "saas-platform.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{- define "saas-platform.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "saas-platform.labels" -}}
helm.sh/chart: {{ include "saas-platform.chart" . }}
{{ include "saas-platform.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{- define "saas-platform.selectorLabels" -}}
app.kubernetes.io/name: {{ include "saas-platform.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "saas-platform.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "saas-platform.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{- define "saas-platform.serviceName" -}}
{{- printf "%s-%s" (include "saas-platform.fullname" .) .serviceName }}
{{- end }}

{{- define "saas-platform.hasDb" -}}
{{- if and .Values.global.database.enabled (ne .serviceName "gateway") }}true{{ else }}false{{ end }}
{{- end }}
