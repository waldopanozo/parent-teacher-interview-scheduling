import { HttpClient } from '@angular/common/http';
import { ScheduleApiService } from './schedule-api.service';

describe('ScheduleApiService', () => {
  it('should construct with HttpClient', () => {
    const http = jasmine.createSpyObj<HttpClient>(['get', 'post', 'put', 'delete', 'patch']);
    const api = new ScheduleApiService(http);
    expect(api).toBeTruthy();
  });
});
