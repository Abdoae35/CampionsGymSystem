# Late-route addendum — incorporated
GET `/api/coaching/classes/members/{member:guid}/attendance` is now included in the principal [Coaching catalog](api/Coaching.md#get-api-coaching-classes-members-member-guid-attendance), current compiler supplement and fresh OpenAPI. It is a completed authenticated Classes.View branch-scoped read with page/pageSize and an explicit branchId query. No separate pending contract is required. Current counts are 191 controller actions, 36 Coaching actions and 192 OpenAPI operations including Development health. Current occurrence validation requires a future start on the same gym-local calendar day. See [release verification](COMPLETENESS_AUDIT.md).

<a id="added-current-route"></a>
## Added current route
The earlier late-route contract is fully integrated in the current Coaching catalog linked above.
